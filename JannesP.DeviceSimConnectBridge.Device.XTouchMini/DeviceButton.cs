using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;

namespace JannesP.DeviceSimConnectBridge.Device.XTouchMini
{
    internal class DeviceButton : IDeviceButton, IDeviceLed
    {
        public DeviceButton(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }

        public DeviceLedState ValidStates { get; } = DeviceLedState.Off | DeviceLedState.On | DeviceLedState.Blinking;
    }
}
