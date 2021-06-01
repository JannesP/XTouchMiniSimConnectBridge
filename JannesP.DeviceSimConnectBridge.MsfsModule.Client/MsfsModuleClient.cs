using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.MsfsModule.Client.Requests;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.DeviceSimConnectBridge.MsfsModule.Client
{
    public class IntervalResultEventArgs : EventArgs
    {
        public IntervalResultEventArgs(int requestId, string lVarName, double value)
        {
            LVarName = lVarName;
            Value = value;
            RequestId = requestId;
        }

        public string LVarName { get; }
        public int RequestId { get; }
        public double Value { get; }
    }

    public class MsfsModuleClient : IDisposable
    {
        private static readonly List<(SimConnectWrapper.SimConnectWrapper, MsfsModuleClient)> _registeredWith = new List<(SimConnectWrapper.SimConnectWrapper, MsfsModuleClient)>();
        private static readonly SemaphoreSlim _semStatic = new SemaphoreSlim(1);

        private readonly Dictionary<int, IntervalRequest> _intervalRequests = new Dictionary<int, IntervalRequest>();

        protected MsfsModuleClient(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            IsValid = true;

            SimConnect = simConnect;
            simConnect.SimConnectClose += SimConnect_SimConnectClose;
            simConnect.ClientDataReceived += SimConnect_ClientDataReceived;

            ReadLVarRequest.SetupChannels(simConnect).Wait();
            ExecuteCalculatorCodeRequest.SetupChannels(simConnect).Wait();
        }

        public event EventHandler<IntervalResultEventArgs>? IntervalResult;

        public bool IsValid { get; private set; }

        internal SimConnectWrapper.SimConnectWrapper SimConnect { get; private set; }

        public static MsfsModuleClient CreateFor(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            _semStatic.Wait();
            try
            {
                MsfsModuleClient? result = _registeredWith.SingleOrDefault(s => s.Item1 == simConnect).Item2;
                if (result == null)
                {
                    result = new MsfsModuleClient(simConnect);
                    _registeredWith.Add((simConnect, result));
                }
                return result;
            }
            finally
            {
                _semStatic.Release();
            }
        }

        public void Dispose()
        {
            lock (_intervalRequests)
            {
                foreach (KeyValuePair<int, IntervalRequest> ir in _intervalRequests)
                {
                    ir.Value.Dispose();
                }
                _intervalRequests.Clear();
            }
        }

        public Task ExecuteCalculatorCode(string code)
        {
            var req = new ExecuteCalculatorCodeRequest(code);
            return req.Execute(SimConnect);
        }

        public Task FireHEvent(string name)
        {
            return ExecuteCalculatorCode($"(>H:{name})");
        }

        public async Task<double> ReadLVar(string name)
        {
            var req = new ReadLVarRequest(name);
            return await req.Execute(SimConnect).ConfigureAwait(false);
        }

        public int StartLVarIntervalUpdates(string name, TimeSpan interval)
        {
            var req = new IntervalRequest(this, name, interval);
            lock (_intervalRequests)
            {
                _intervalRequests.Add(req.RequestId, req);
            }
            return req.RequestId;
        }

        public void StopLVarIntervalUpdates(int intervalId)
        {
            lock (_intervalRequests)
            {
                if (_intervalRequests.TryGetValue(intervalId, out IntervalRequest request))
                {
                    _intervalRequests.Remove(intervalId);
                    request.Stop();
                    request.Dispose();
                }
            }
        }

        private void FireIntervalRequestResult(int requestId, string lvarName, double value)
        {
            IntervalResult?.Invoke(this, new IntervalResultEventArgs(requestId, lvarName, value));
        }

        private void SimConnect_ClientDataReceived(object sender, SimConnectWrapper.EventArgs.ClientDataReceivedEventArgs e)
        {
            SIMCONNECT_RECV_CLIENT_DATA data = e.RecvClientData;
            switch (data.dwRequestID)
            {
                case ReadLVarRequest.RequestId:
                    ReadLVarRequest.HandleResponse(data.dwData);
                    break;
            }
        }

        private void SimConnect_SimConnectClose(object sender, EventArgs e)
        {
            _semStatic.Wait();
            try
            {
                IsValid = false;
                int index = _registeredWith.FindIndex(s => s.Item1 == SimConnect);
                _registeredWith.RemoveAt(index);
            }
            finally
            {
                _semStatic.Release();
            }
            Dispose();
        }

        private class IntervalRequest : IDisposable
        {
            private static int _nextRequestId = 0;

            public IntervalRequest(MsfsModuleClient client, string lVarName, TimeSpan interval)
            {
                Client = client;
                LVarName = lVarName;
                Interval = interval;
                CancellationTokenSource = new CancellationTokenSource();
                RequestId = Interlocked.Increment(ref _nextRequestId);
                _ = RunLoop(CancellationTokenSource.Token);
            }

            public MsfsModuleClient Client { get; }
            public TimeSpan Interval { get; }
            public string LVarName { get; }
            public int RequestId { get; }
            private CancellationTokenSource CancellationTokenSource { get; }

            public void Dispose() => CancellationTokenSource.Dispose();

            public void Stop()
            {
                CancellationTokenSource.Cancel();
            }

            private async Task RunLoop(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        double value = await Client.ReadLVar(LVarName);
                        if (!token.IsCancellationRequested)
                        {
                            Client.FireIntervalRequestResult(RequestId, LVarName, value);
                            try
                            {
                                await Task.Delay(Interval, token);
                            }
                            catch (TaskCanceledException) { /* ignore */ }
                        }
                    }
                    catch (TimeoutException) { /* just try again */ }
                }
            }
        }
    }
}