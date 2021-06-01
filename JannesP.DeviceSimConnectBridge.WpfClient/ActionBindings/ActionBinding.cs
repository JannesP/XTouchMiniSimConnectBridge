using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;
using JannesP.DeviceSimConnectBridge.Device;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings
{
    [DataContract]
    public abstract class ActionBinding
    {
        [DataMember]
        public string? Description { get; set; }

        [DataMember]
        public int? DeviceControlId { get; set; }

        public bool IsEnabled { get; protected set; }

        protected IDevice? Device { get; private set; }
        protected IServiceProvider? ServiceProvider { get; private set; }

        public virtual void Disable()
        {
            Device = null;
            IsEnabled = false;
        }

        [MemberNotNull(nameof(Device), nameof(ServiceProvider))]
        public virtual void Enable(IServiceProvider serviceProvider, IDevice device)
        {
            IsEnabled = true;
            ServiceProvider = serviceProvider;
            Device = device;
        }

        public abstract bool IsEmpty();
    }
}