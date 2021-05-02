using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.Device.XTouchMini;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Repositories
{
    public class DeviceRepository
    {
        private readonly Dictionary<string, IDevice> _availableDevices = new();

        public DeviceRepository()
        {
            var mini = new XTouchMiniDevice();
            AddDevice(mini);
        }

        public IReadOnlyDictionary<string, IDevice> AvailableDevices => _availableDevices;
        public void AddDevice(IDevice device) => _availableDevices.Add(device.TechnicalDeviceIdentifier, device);

        public IDevice? FindDeviceByTechnicalIdentifier(string identifier) => AvailableDevices.GetValueOrDefault(identifier);
    }
}
