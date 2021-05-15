using System;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions
{
    public interface ISimBoolSourceAction : IBindableAction
    {
        event EventHandler<SimDataReceivedEventArgs<bool>>? SimBoolReceived;
    }

    public class SimDataReceivedEventArgs<TData> : EventArgs
    {
        public SimDataReceivedEventArgs(TData newValue)
        {
            NewValue = newValue;
        }

        public TData NewValue { get; set; }
    }
}