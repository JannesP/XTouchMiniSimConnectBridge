using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.SimConnectWrapper
{
    internal interface ISimConnectPreparator
    {
        void RegisterDataDefinition(SimConnectDataDefinition dataDefinition);
    }

    public class SimConnectRequestObjectByType<TRes> : SimConnectRequest<TRes>
    {
        private enum PrivateDummy { }

        public enum SimConnectSimobjectType
        {
            USER,
            ALL,
            AIRCRAFT,
            HELICOPTER,
            BOAT,
            GROUND
        }

        public SimConnectDataDefinition DataDefinition { get; }
        public SimConnectSimobjectType ObjectType { get; }

        public SimConnectRequestObjectByType(SimConnectDataDefinition dataDefinition, SimConnectSimobjectType objectType = SimConnectSimobjectType.USER) : base()
        {
            DataDefinition = dataDefinition;
            ObjectType = objectType;
        }

        internal override void PrepareRequest(ISimConnectPreparator preparator, SimConnect simConnect)
        {
            preparator.RegisterDataDefinition(DataDefinition);
        }

        internal override void ExecuteRequest(uint requestId, SimConnect simConnect)
        {
            RequestId = requestId;
            simConnect.RequestDataOnSimObjectType((PrivateDummy)requestId, (PrivateDummy)DataDefinition.DefinitionId, 0, (SIMCONNECT_SIMOBJECT_TYPE)ObjectType);
            SimConnectNative.SimConnect_GetLastSentPacketID(simConnect.GetSimConnectHandle(), out uint sendId);
            SendId = sendId;
        }
    }

    public abstract class SimConnectRequest<TRes> : SimConnectRequest
    {
        protected SimConnectRequest() : base(typeof(TRes))
        {
            //run continuations asynchronously so consumers cannot deadlock the semaphore in SimConnectWrapper
            TaskCompletionSource = new TaskCompletionSource<TRes>(TaskCreationOptions.RunContinuationsAsynchronously);
        }
        
        internal TaskCompletionSource<TRes> TaskCompletionSource { get; }
        internal override void SetResult(object result)
        {
            TaskCompletionSource.SetResult((TRes)result);
        }
        internal override void SetException(Exception ex)
        {
            TaskCompletionSource.SetException(ex);
        }
    }

    public abstract class SimConnectRequest
    {
        protected SimConnectRequest(Type resultType)
        {
            ResultType = resultType;
        }
        public Type ResultType { get; }

        public uint RequestId { get; protected set; }
        public uint SendId { get; protected set; }
        internal abstract void SetResult(object result);
        internal abstract void SetException(Exception ex);
        internal abstract void PrepareRequest(ISimConnectPreparator preparator, SimConnect simConnect);
        internal abstract void ExecuteRequest(uint requestId, SimConnect simConnect);
    }
}
