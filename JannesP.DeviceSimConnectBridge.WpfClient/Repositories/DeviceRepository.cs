using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.Device.XTouchMini;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Repositories
{
    public class DeviceRepository : IDisposable
    {
        private readonly List<IDevice> _availableDevices = new();

        public DeviceRepository()
        {
            var mini = new XTouchMiniDevice();
            AddDevice(mini);
        }

        public event EventHandler? DeviceListChanged;

        public IReadOnlyList<IDevice> AvailableDevices => _availableDevices;

        public void AddDevice(IDevice device)
        {
            _availableDevices.Add(device);
            DeviceListChanged?.Invoke(this, EventArgs.Empty);
        }

        public void Dispose()
        {
            foreach (IDevice dev in _availableDevices)
            {
                if (dev.IsConnected)
                {
                    dev.DisconnectAsync().Wait();
                }
            }
        }

        public void RemoveDevice(IDevice device)
        {
            _availableDevices.Remove(device);
            DeviceListChanged?.Invoke(this, EventArgs.Empty);
        }

        public IDevice? TryFindDevice(string type, string? id) => AvailableDevices.FirstOrDefault(d => d.DeviceType == type && d.DeviceId == id);

        public bool TryFindDevice(string type, string? id, [NotNullWhen(true)] out IDevice? device)
        {
            device = AvailableDevices.FirstOrDefault(d => d.DeviceType == type && d.DeviceId == id);
            return device != null;
        }
    }
}