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