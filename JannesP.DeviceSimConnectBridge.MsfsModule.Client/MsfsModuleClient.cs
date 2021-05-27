using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.MsfsModule.Client.Requests;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.DeviceSimConnectBridge.MsfsModule.Client
{
    public class MsfsModuleClient : IDisposable
    {
        private static readonly List<SimConnectWrapper.SimConnectWrapper> _registeredWith = new List<SimConnectWrapper.SimConnectWrapper>();
        private static readonly SemaphoreSlim _semStatic = new SemaphoreSlim(1);

        public MsfsModuleClient(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            _semStatic.Wait();
            try
            {
                if (_registeredWith.Contains(simConnect)) throw new Exception("Only one MsfsModuleClient can be created for a given SimConnectWrapper.");
                _registeredWith.Add(simConnect);
                IsValid = true;
            }
            finally
            {
                _semStatic.Release();
            }

            SimConnect = simConnect;
            simConnect.SimConnectClose += SimConnect_SimConnectClose;
            simConnect.ClientDataReceived += SimConnect_ClientDataReceived;

            ReadLVarRequest.SetupChannels(simConnect).Wait();
            ExecuteCalculatorCodeRequest.SetupChannels(simConnect).Wait();
        }

        public bool IsValid { get; private set; }

        internal SimConnectWrapper.SimConnectWrapper SimConnect { get; private set; }

        public void Dispose()
        {
            //TODO: dispose all running requests
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
                _registeredWith.Remove(SimConnect);
            }
            finally
            {
                _semStatic.Release();
            }
        }
    }
}