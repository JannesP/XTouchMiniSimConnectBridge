using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;

namespace JannesP.DeviceSimConnectBridge.XTouchMiniDevice
{
    class DeviceFader : IDeviceFader
    {
        public DeviceFader(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}
