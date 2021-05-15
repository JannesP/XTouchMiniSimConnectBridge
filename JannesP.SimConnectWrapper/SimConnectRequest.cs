using System;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.SimConnectWrapper
{
    internal interface ISimConnectPreparator
    {
        void RegisterDataDefinition(SimConnectDataDefinition dataDefinition);
    }

    public abstract class SimConnectRequest<TRes> : SimConnectRequest
    {
        protected SimConnectRequest() : base(typeof(TRes))
        {
            //run continuations asynchronously so consumers cannot deadlock the semaphore in SimConnectWrapper
            TaskCompletionSource = new TaskCompletionSource<TRes>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        internal TaskCompletionSource<TRes> TaskCompletionSource { get; }

        internal override void SetException(Exception ex)
        {
            TaskCompletionSource.SetException(ex);
        }

        internal override void SetResult(object result)
        {
            TaskCompletionSource.SetResult((TRes)result);
        }
    }

    public abstract class SimConnectRequest
    {
        protected SimConnectRequest(Type resultType)
        {
            ResultType = resultType;
        }

        public uint RequestId { get; protected set; }
        public Type ResultType { get; }
        public uint SendId { get; protected set; }

        internal abstract void ExecuteRequest(uint requestId, SimConnect simConnect);

        internal abstract void PrepareRequest(ISimConnectPreparator preparator, SimConnect simConnect);

        internal abstract void SetException(Exception ex);

        internal abstract void SetResult(object result);
    }

    public class SimConnectRequestObjectByType<TRes> : SimConnectRequest<TRes>
    {
        public SimConnectRequestObjectByType(SimConnectDataDefinition dataDefinition, SimConnectSimobjectType objectType = SimConnectSimobjectType.USER) : base()
        {
            DataDefinition = dataDefinition;
            ObjectType = objectType;
        }

        public enum SimConnectSimobjectType
        {
            USER,
            ALL,
            AIRCRAFT,
            HELICOPTER,
            BOAT,
            GROUND
        }

        private enum PrivateDummy { }

        public SimConnectDataDefinition DataDefinition { get; }
        public SimConnectSimobjectType ObjectType { get; }

        internal override void ExecuteRequest(uint requestId, SimConnect simConnect)
        {
            RequestId = requestId;
            simConnect.RequestDataOnSimObjectType((PrivateDummy)requestId, (PrivateDummy)DataDefinition.DefinitionId, 0, (SIMCONNECT_SIMOBJECT_TYPE)ObjectType);
            SimConnectNative.SimConnect_GetLastSentPacketID(simConnect.GetSimConnectHandle(), out uint sendId);
            SendId = sendId;
        }

        internal override void PrepareRequest(ISimConnectPreparator preparator, SimConnect simConnect)
        {
            preparator.RegisterDataDefinition(DataDefinition);
        }
    }
}