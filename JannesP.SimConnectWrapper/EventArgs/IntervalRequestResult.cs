namespace JannesP.SimConnectWrapper.EventArgs
{
    public class IntervalRequestResultEventArgs : System.EventArgs
    {
        public IntervalRequestResultEventArgs(object? result, int requestId, SimConnectDataDefinition dataDefinition)
        {
            Result = result;
            RequestId = requestId;
            DataDefinition = dataDefinition;
        }

        public SimConnectDataDefinition DataDefinition { get; }
        public int RequestId { get; }
        public object? Result { get; }
    }
}