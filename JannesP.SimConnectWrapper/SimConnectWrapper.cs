using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using JannesP.SimConnectWrapper.EventArgs;
using Microsoft.FlightSimulator.SimConnect;
using static JannesP.SimConnectWrapper.Win32;

namespace JannesP.SimConnectWrapper
{
    public class SimConnectWrapper : IDisposable, ISimConnectPreparator
    {
        private const uint _wmAppSimConnect = (uint)WindowMessage.WM_APP + 0x0239;

        private readonly string _appName;

        private readonly Dictionary<int, SimConnectDataDefinition> _registeredDataDefinitions = new Dictionary<int, SimConnectDataDefinition>();

        private readonly Dictionary<string, int> _registeredEventDefinitions = new Dictionary<string, int>();

        private readonly Dictionary<int, (Task, CancellationTokenSource)> _registeredIntervalRequests = new Dictionary<int, (Task, CancellationTokenSource)>();

        private readonly Dictionary<uint, SimConnectRequest> _requests = new Dictionary<uint, SimConnectRequest>();

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1);

        private int _intervalRequestCounter = 0;

        private int _isDisposed = 0;

        private int _lastRequestId = 0;

        private MessagePumpWindow? _msgPump;

        private SimConnect? _simConnect;

        public SimConnectWrapper(string applicationName)
        {
            _appName = applicationName;
        }

        public event EventHandler<ClientDataReceivedEventArgs>? ClientDataReceived;

        public event EventHandler<IntervalRequestResultEventArgs>? IntervalRequestResult;

        public event EventHandler? SimConnectClose;

        public event EventHandler? SimConnectOpen;

        private enum EventGroup
        {
            Dummy,
        }

        private enum PrivateDummy { }

        public bool IsOpen { get; private set; }

        public async Task AddToClientDataDefinition(uint defineId, uint dwOffset, uint dwSizeOrType, float fEpsilon = 0.0f, uint datumId = uint.MaxValue)
        {
            await _semaphore.WaitAsync();
            try
            {
                _simConnect?.AddToClientDataDefinition((PrivateDummy)defineId, dwOffset, dwSizeOrType, fEpsilon, datumId);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task AddToClientDataDefinition<TData>(uint defineId, uint dwOffset = 0) where TData : struct
        {
            await AddToClientDataDefinition(defineId, dwOffset, (uint)Marshal.SizeOf<TData>());
            await RegisterClientDataDefineStruct<TData>(defineId);
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

        public async Task Disconnect()
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                IsOpen = false;
                if (_simConnect != null)
                {
                    SimConnect sc = _simConnect;
                    _simConnect = null;
                    sc.Dispose();
                    SimConnectClose?.Invoke(this, new System.EventArgs());
                }

                _registeredDataDefinitions.Clear();
                _registeredEventDefinitions.Clear();
                foreach (KeyValuePair<uint, SimConnectRequest> req in _requests)
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

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _isDisposed, 1) == 0)
            {
                Disconnect().Wait();
                if (_msgPump != null)
                {
                    _msgPump.MessagePumpDestroyed -= OnMsgPump_MessagePumpDestroyed;
                    _msgPump.Dispose();
                }
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
                                    TRes result = await RequestObjectByType<TRes>(dataDefinition).ConfigureAwait(false);
                                    IntervalRequestResult?.Invoke(this, new IntervalRequestResultEventArgs(result, intervalId, dataDefinition));
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"Exception while requesting update for IntervalRequest: {ex}");
                                }
                            }
                        }
                        await Task.Delay(requestToRequestMs, cts.Token).ConfigureAwait(false);
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

        public async Task MapClientDataNameToID(string clientDataName, uint clientDataId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _simConnect?.MapClientDataNameToID(clientDataName, (PrivateDummy)clientDataId);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task RegisterClientDataDefineStruct<T>(uint defineId)
        {
            await _semaphore.WaitAsync();
            try
            {
                _simConnect?.RegisterStruct<SIMCONNECT_RECV_CLIENT_DATA, T>((PrivateDummy)defineId);
            }
            finally
            {
                _semaphore.Release();
            }
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

        public async Task<TRes> Request<TRes>(SimConnectRequest<TRes> request)
        {
            bool requestStarted = false;
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                if (IsOpen && _simConnect != null)
                {
                    int newRequestId = ++_lastRequestId;
                    _requests.Add((uint)newRequestId, request);
                    request.PrepareRequest(this, _simConnect);
                    request.ExecuteRequest((uint)newRequestId, _simConnect);
                    requestStarted = true;
                }
            }
            finally
            {
                _semaphore.Release();
            }
            if (requestStarted)
            {
                return await request.TaskCompletionSource.Task.ConfigureAwait(false);
            }
            else
            {
                throw new InvalidOperationException("SimConnect is not connected!");
            }
        }

        public async Task RequestClientData(uint clientDataId, uint requestId, uint defineId, SIMCONNECT_CLIENT_DATA_PERIOD period = SIMCONNECT_CLIENT_DATA_PERIOD.ONCE, SIMCONNECT_CLIENT_DATA_REQUEST_FLAG flags = SIMCONNECT_CLIENT_DATA_REQUEST_FLAG.DEFAULT, uint origin = 0, uint interval = 0, uint limit = 0)
        {
            await _semaphore.WaitAsync();
            try
            {
                _simConnect?.RequestClientData((PrivateDummy)clientDataId, (PrivateDummy)requestId, (PrivateDummy)defineId, period, flags, origin, interval, limit);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public Task<TRes> RequestObjectByType<TRes>(SimConnectDataDefinition dataDefinition) => Request(new SimConnectRequestObjectByType<TRes>(dataDefinition));

        /// <summary>
        /// Sends a simple SimEvent to the connected sim. For a list of possible values see the <see href="https://docs.flightsimulator.com/html/Programming_Tools/SimVars/Legacy_Event_IDs.htm">list in the MSFS docs</see>.
        /// </summary>
        /// <param name="simEventName">The name of the Sim</param>
        /// <returns></returns>
        public async Task SendEvent(string simEventName, uint dwData = 0u)
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
                    await Task.Run(() => _simConnect.TransmitClientEvent(0, (PrivateDummy)clientEventId, dwData, EventGroup.Dummy, SIMCONNECT_EVENT_FLAG.GROUPID_IS_PRIORITY));
                }
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public async Task SetClientData(uint clientDataId, uint defineId, SIMCONNECT_CLIENT_DATA_SET_FLAG flags, object dataSet)
        {
            await _semaphore.WaitAsync();
            try
            {
                _simConnect?.SetClientData((PrivateDummy)clientDataId, (PrivateDummy)defineId, flags, 0, dataSet);
            }
            finally
            {
                _semaphore.Release();
            }
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
                    _msgPump.MessagePumpDestroyed += OnMsgPump_MessagePumpDestroyed;
                }
                if (_simConnect == null)
                {
                    try
                    {
                        await Task.Run(() =>
                        {
                            _simConnect = new SimConnect(_appName, _msgPump.Handle, _wmAppSimConnect, null, 0);
                            _simConnect.OnRecvOpen += OnSimConnect_OnRecvOpen;
                            _simConnect.OnRecvQuit += OnSimConnect_OnRecvQuit;
                            _simConnect.OnRecvClientData += OnSimConnect_OnRecvClientData;
                            _simConnect.OnRecvException += OnSimConnect_OnRecvException;
                            _simConnect.OnRecvSimobjectDataBytype += OnSimConnect_OnRecvSimobjectDataBytype;
                        }).ConfigureAwait(false);
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

        //since we can't rescue from this state we just close the object
        private void OnMsgPump_MessagePumpDestroyed(object sender, System.EventArgs e) => Dispose();

        private void OnSimConnect_OnRecvClientData(SimConnect sender, SIMCONNECT_RECV_CLIENT_DATA data)
            => ClientDataReceived?.Invoke(this, new ClientDataReceivedEventArgs(data));

        private async void OnSimConnect_OnRecvException(SimConnect sender, SIMCONNECT_RECV_EXCEPTION data)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            try
            {
                SimConnectRequest? request = _requests.FirstOrDefault(kvp => kvp.Value.SendId == data.dwSendID).Value;
                if (request != null)
                {
                    _requests.Remove(request.RequestId);
                    request.SetException(new Exception($"Request error from SimConnect (dwException: {data.dwException})"));
                }
                else
                {
                    Console.WriteLine("Unhandled OnSimConnect_OnRecvException!");
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private void OnSimConnect_OnRecvOpen(SimConnect sender, SIMCONNECT_RECV_OPEN data)
        {
            Console.WriteLine("OnSimConnect_OnRecvOpen");
            IsOpen = true;
            SimConnectOpen?.Invoke(this, new System.EventArgs());
        }

        private async void OnSimConnect_OnRecvQuit(SimConnect sender, SIMCONNECT_RECV data)
        {
            Console.WriteLine("_simConnect_OnRecvQuit");
            await Disconnect().ConfigureAwait(false);
        }

        private void OnSimConnect_OnRecvSimobjectDataBytype(SimConnect sender, SIMCONNECT_RECV_SIMOBJECT_DATA_BYTYPE data)
        {
            if (_requests.TryGetValue(data.dwRequestID, out SimConnectRequest request))
            {
                object? result = data.dwData?.FirstOrDefault();
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
                    catch (Exception ex)
                    {
                        request.SetException(ex);
                    }
                }
                _requests.Remove(data.dwRequestID);
            }
        }

        private IntPtr WndProc(IntPtr hWnd, WindowMessage msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case (WindowMessage)_wmAppSimConnect:
                    try
                    {
                        _simConnect?.ReceiveMessage();
                    }
                    catch (COMException)
                    {
                        //dispose if there's a COM Exception. The application could just reconnect if the connection is still required.
                        Dispose();
                    }
                    break;
            }
            return IntPtr.Zero;
        }
    }
}