using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.SimConnectWrapper.EventArgs
{
    public class ClientDataReceivedEventArgs : System.EventArgs
    {
        public ClientDataReceivedEventArgs(SIMCONNECT_RECV_CLIENT_DATA recvClientData)
        {
            RecvClientData = recvClientData;
        }

        public SIMCONNECT_RECV_CLIENT_DATA RecvClientData { get; }
    }
}