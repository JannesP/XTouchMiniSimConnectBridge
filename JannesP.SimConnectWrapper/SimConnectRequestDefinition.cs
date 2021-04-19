using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.SimConnectWrapper
{
    public abstract class SimConnectRequestDefinition
    {
        public enum RequestType
        {
            SimConnectByType,
            CustomExecuteCalcCode,
            CustomRequestLVar,
        }

        internal abstract void ExecuteRequest(SimConnect simConnect);
    }

    
}
