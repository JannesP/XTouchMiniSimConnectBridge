using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;

namespace JannesP.DeviceSimConnectBridge.Device.XTouchMini
{
    public class DeviceLed : IDeviceLed
    {
        public DeviceLed(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public DeviceLedState ValidStates { get; } = DeviceLedState.Off | DeviceLedState.On | DeviceLedState.Blinking;

        public int Id { get; }

        public string Name { get; }
    }
}
