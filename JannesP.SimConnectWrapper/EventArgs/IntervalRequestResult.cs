using System;
using System.Collections.Generic;
using System.Text;

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

        public object? Result { get; }
        public int RequestId { get; }
        public SimConnectDataDefinition DataDefinition { get; }
    }
}
