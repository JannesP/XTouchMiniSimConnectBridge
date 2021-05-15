using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings
{
    [DataContract]
    public abstract class ActionBinding
    {
        protected IDevice? Device { get; private set; }
        protected IServiceProvider? ServiceProvider { get; private set; }
        [DataMember]
        public int? DeviceControlId { get; set; }
        [DataMember]
        public string? Description { get; set; }

        [MemberNotNull(nameof(Device), nameof(ServiceProvider))]
        public virtual void Enable(IServiceProvider serviceProvider, IDevice device)
        {
            ServiceProvider = serviceProvider;
            Device = device;
        }
        public virtual void Disable()
        {
            Device = null;
        }
        public abstract bool IsEmpty();
    }
}
