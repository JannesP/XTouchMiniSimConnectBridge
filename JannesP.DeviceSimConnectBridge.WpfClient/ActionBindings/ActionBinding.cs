using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings
{
    public abstract class ActionBinding
    {
        protected IDevice? Device { get; private set; }
        protected IServiceProvider? ServiceProvider { get; private set; }
        public int? DeviceControlId { get; set; }
        public string? Description { get; set; }

        public virtual void Enable(IServiceProvider serviceProvider, IDevice device)
        {
            ServiceProvider = serviceProvider;
            Device = device;
        }
        public virtual void Disable()
        {
            Device = null;
        }
    }
}
