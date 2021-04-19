using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JannesP.SimConnectWrapper.EventArgs;
using Microsoft.FlightSimulator.SimConnect;
using static JannesP.SimConnectWrapper.Win32;

namespace JannesP.SimConnectWrapper
{
    public class SimConnectWrapper : IDisposable, ISimConnectPreparator
    {
        private enum PrivateDummy { }
        private enum EventGroup
        {
            Dummy,
        }

        private const uint WM_APP_SIMCONNECT = (uint)WindowMessage.WM_APP + 0x0239;

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1); 

        private Dictionary<int, SimConnectDataDefinition> _registeredDataDefinitions = new Dictionary<int, SimConnectDataDefinition>();
        private Dictionary<string, int> _registeredEventDefinitions = new Dictionary<string, int>();
        private Dictionary<int, (Task, CancellationTokenSource)> _registeredIntervalRequests = new Dictionary<int, (Task, CancellationTokenSource)>();
        private Dictionary<uint, SimConnectRequest> _requests = new Dictionary<uint, SimConnectRequest>();
        private readonly string _appName;
        private MessagePumpWindow? _msgPump;
        private SimConnect? _simConnect;
        private int _lastRequestId = 0;
        private int _isDisposed = 0;
        private int _intervalRequestCounter = 0;        

        public event EventHandler? SimConnectOpen;
        public event EventHandler? SimConnectClose;
        public event EventHandler<IntervalRequestResultEventArgs>? IntervalRequestResult;

        public bool IsOpen { get; private set; }

        public SimConnectWrapper(string applicationName)
        {
            _appName = applicationName;
        }

        public async Task<bool> TryConnect()
        {
            if (!await _semaphore.WaitAsync(2000).ConfigureAwait(false))
            {
                //something is going on, connect failed
                return false;
            }
            try
            {
                if (_isDisposed == 1) throw new InvalidOperationException("Cannot work with this disposed object, sorry.");
                if (_msgPump == null)
                {
                    _msgPump = new MessagePumpWindow(WndProc);
                    await _msgPump.Create().ConfigureAwait(false);
                    _msgPump.MessagePumpDestroyed += _msgPump_MessagePumpDestroyed;
                }
                if (_simConnect == null)
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            _simConnect = new SimConnect(_appName, _msgPump.Handle, WM_APP_SIMCONNECT, null, 0);
                            _simConnect.OnRecvOpen += _simConnect_OnRecvOpen;
                            _simConnect.OnRecvQuit += _simConnect_OnRecvQuit;
                            _simConnect.OnRecvClientData += _simConnect_OnRecvClientData;
                            _simConnect.OnRecvException += _simConnect_OnRecvException;
                            _simConnect.OnRecvSimobjectDataBytype += _simConnect_OnRecvSimobjectDataBytype;

                        //_simConnect?.AddToDataDefinition(Test.PlaneHeadingDegrees, "PLANE HEADING DEGREES MAGNETIC", "degree", SIMCONNECT_DATATYPE.FLOAT64, 0.0f, SimConnect.SIMCONNECT_UNUSED);
                        //_simConnect?.RegisterDataDefineStruct<double>(Test.PlaneHeadingDegrees);
                    });
                    }
                    catch (COMException)
                    {
                        return false;
                    }
                }
                return true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task<TRes> RequestObjectByType<TRes>(SimConnectDataDefinition dataDefinition)
        {
            return Request(new SimConnectRequestObjectByType<TRes>(dataDefinition));
        }

        public async Task<TRes> Request<TRes>(SimConnectRequest<TRes> request)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (IsOpen && _simConnect != null)
                {
                    int newRequestId = ++_lastRequestId;
                    _requests.Add((uint)newRequestId, request);
                    request.PrepareRequest(this, _simConnect);
                    request.ExecuteRequest((uint)newRequestId, _simConnect);
                    Console.WriteLine($"Sending message ID: {request.RequestId}");
                    return await request.TaskCompletionSource.Task.ConfigureAwait(false);
                }
                else
                {
                    throw new InvalidOperationException("SimConnect is not connected!");
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task<int> IntervalRequestObjectByType<TRes>(int requestToRequestMs, SimConnectDataDefinition dataDefinition)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            int intervalId;
            try
            {
                intervalId = ++_intervalRequestCounter;
                var cts = new CancellationTokenSource();
                var intervalTask = Task.Run(async () =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        if (IntervalRequestResult != null)
                        {
                            if (IsOpen)
                            {
                                try
                                {
                                    TRes result = await RequestObjectByType<TRes>(dataDefinition);
                                    IntervalRequestResult?.Invoke(this, new IntervalRequestResultEventArgs(result, intervalId, dataDefinition));
                                }
                                catch(Exception ex)
                                {
                                    Console.WriteLine($"Exception while requesting update for IntervalRequest: {ex}");
                                }
                            }
                        }
                        await Task.Delay(requestToRequestMs, cts.Token);
                    }
                }, cts.Token);
            
                _registeredIntervalRequests.Add(intervalId, (intervalTask, cts));
            }
            finally
            {
                _semaphore.Release();
            }
            return intervalId;
        }

        public async Task CancelIntervalRequest(int id)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_registeredIntervalRequests.TryGetValue(id, out (Task, CancellationTokenSource) value))
                {
                    value.Item2.Cancel();
                    value.Item2.Dispose();
                    _registeredIntervalRequests.Remove(id);
                }
            }
            finally
            {
                _semaphore.Release();
            }

        }

        /// <summary>
        /// Sends a simple SimEvent to the connected sim. For a list of possible values see the <see href="https://docs.flightsimulator.com/html/Programming_Tools/SimVars/Legacy_Event_IDs.htm">list in the MSFS docs</see>.
        /// </summary>
        /// <param name="simEventName">The name of the Sim</param>
        /// <returns></returns>
        public async Task SendEvent(string simEventName)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (IsOpen && _simConnect != null)
                {
                    //get clientEventId by either registering a new one or getting it from the dictionary
                    int clientEventId;
                    if (!_registeredEventDefinitions.TryGetValue(simEventName, out clientEventId))
                    {
                        clientEventId = _registeredEventDefinitions.Count + 1;
                        _registeredEventDefinitions.Add(simEventName, clientEventId);

                        _simConnect.MapClientEventToSimEvent((PrivateDummy)clientEventId, simEventName);
                        _simConnect.AddClientEventToNotificationGroup(EventGroup.Dummy, (PrivateDummy)clientEventId, false);
                    }
                    _simConnect.TransmitClientEvent(0, (PrivateDummy)clientEventId, 0, EventGroup.Dummy, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY);
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task Disconnect()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                IsOpen = false;
                SimConnectClose?.Invoke(this, new System.EventArgs());
                _simConnect?.Dispose();
                _simConnect = null;
                _registeredDataDefinitions.Clear();
                _registeredEventDefinitions.Clear();
                foreach (var req in _requests)
                {
                    try
                    {
                        req.Value.SetException(new OperationCanceledException("The SimConnect client handling this request has been disposed."));
                    }
                    catch { /* ignore since we're disposing anyways */ }
                }
                _requests.Clear();
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void _msgPump_MessagePumpDestroyed(object sender, System.EventArgs e)
        {
            //since we can't rescue from this state we just close the object
            Dispose();
        }

        private void _simConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (_requests.TryGetValue(data.dwRequestID, out SimConnectRequest request))
            {
                var result = data.dwData?.FirstOrDefault();
                if (result == null)
                {
                    request.SetException(new Exception("Request got no result."));
                }
                else
                {
                    try
                    {
                        request.SetResult(result);
                    }
                    catch(Exception ex)
                    {
                        request.SetException(ex);
                    }
                }
                _requests.Remove(data.dwRequestID);
            }
            Console.WriteLine($"_simConnect_OnRecvSimobjectDataBytype {data.dwRequestID} - {data.dwData?.FirstOrDefault()}");
        }

        private async void _simConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                var request = _requests.FirstOrDefault(kvp => kvp.Value.SendId == data.dwSendID).Value;
                if (request != null)
                {
                    _requests.Remove(request.RequestId);
                    request.SetException(new Exception($"Request error from SimConnect (dwException: {data.dwException})"));
                }
                else
                {
                    Console.WriteLine("Unhandled _simConnect_OnRecvException!");
                }
            }
            catch
            {
                _semaphore.Release();
                throw;
            }
        }

        private async void _simConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("_simConnect_OnRecvQuit");
            await Disconnect().ConfigureAwait(false);
        }

        private void _simConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("_simConnect_OnRecvOpen");
            IsOpen = true;
            SimConnectOpen?.Invoke(this, new System.EventArgs());
        }

        private void _simConnect_OnRecvClientData(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
        {
            Console.WriteLine("_simConnect_OnRecvClientData");
        }

        private IntPtr WndProc(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            Console.WriteLine($"[wndProc] {hWnd:X8} {msg} {wParam} {lParam}");
            switch (msg)
            {
                case (WindowMessage)WM_APP_SIMCONNECT:
                    _simConnect?.ReceiveMessage();
                    break;
            }
            return IntPtr.Zero;
        }        

        public void RegisterDataDefinition(SimConnectDataDefinition dataDefinition)
        {
            if (_registeredDataDefinitions.TryGetValue(dataDefinition.DefinitionId, out SimConnectDataDefinition def))
            {
                if (!def.Equals(dataDefinition))
                {
                    throw new InvalidOperationException($"{dataDefinition.DefinitionId} is already defined for '{def.DataName}'. Requesting DataDefinition: {dataDefinition.DataName}");
                }
            }
            else
            {
                _registeredDataDefinitions.Add(dataDefinition.DefinitionId, dataDefinition);
                _simConnect?.AddToDataDefinition((PrivateDummy)dataDefinition.DefinitionId, dataDefinition.DataName, dataDefinition.UnitName, dataDefinition.SimConnectDataType, 0, SimConnect.SIMCONNECT_UNUSED);
                switch (dataDefinition.SimConnectDataType)
                {                    
                    case SIMCONNECT_DATATYPE.INT32:
                        _simConnect?.RegisterDataDefineStruct<int>((PrivateDummy)dataDefinition.DefinitionId);
                        break;
                    case SIMCONNECT_DATATYPE.INT64:
                        _simConnect?.RegisterDataDefineStruct<long>((PrivateDummy)dataDefinition.DefinitionId);
                        break;
                    case SIMCONNECT_DATATYPE.FLOAT32:
                        _simConnect?.RegisterDataDefineStruct<float>((PrivateDummy)dataDefinition.DefinitionId);
                        break;
                    case SIMCONNECT_DATATYPE.FLOAT64:
                        _simConnect?.RegisterDataDefineStruct<double>((PrivateDummy)dataDefinition.DefinitionId);
                        break;
                    case SIMCONNECT_DATATYPE.STRING8:
                    case SIMCONNECT_DATATYPE.STRING32:
                    case SIMCONNECT_DATATYPE.STRING64:
                    case SIMCONNECT_DATATYPE.STRING128:
                    case SIMCONNECT_DATATYPE.STRING256:
                    case SIMCONNECT_DATATYPE.STRING260:
                    case SIMCONNECT_DATATYPE.STRINGV:
                        _simConnect?.RegisterDataDefineStruct<string>((PrivateDummy)dataDefinition.DefinitionId);
                        break;
                    case SIMCONNECT_DATATYPE.INITPOSITION:
                    case SIMCONNECT_DATATYPE.MARKERSTATE:
                    case SIMCONNECT_DATATYPE.WAYPOINT:
                    case SIMCONNECT_DATATYPE.LATLONALT:
                    case SIMCONNECT_DATATYPE.XYZ:
                    case SIMCONNECT_DATATYPE.MAX:
                    case SIMCONNECT_DATATYPE.INVALID:
                    default:
                        throw new Exception("Unsupported datatype.");
                }
            }
        }

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                Disconnect().Wait();
                if (_msgPump != null)
                {
                    _msgPump.MessagePumpDestroyed -= _msgPump_MessagePumpDestroyed;
                    _msgPump.Dispose();
                }
            }
        }
    }
}
