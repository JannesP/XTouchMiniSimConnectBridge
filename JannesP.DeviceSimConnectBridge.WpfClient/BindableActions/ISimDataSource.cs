using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions
{
    public interface ISimBoolSourceAction : IBindableAction
    {
        event EventHandler<SimDataReceivedEventArgs<bool>>? SimBoolReceived;
    }

    public class SimDataReceivedEventArgs<TData> : EventArgs
    {
        public TData NewValue { get; set; }

        public SimDataReceivedEventArgs(TData newValue)
        {
            NewValue = newValue;
        }
    }
}
