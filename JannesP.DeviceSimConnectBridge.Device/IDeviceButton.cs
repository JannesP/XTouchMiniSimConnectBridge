using System;

namespace JannesP.DeviceSimConnectBridge.Device
{
    public interface IDeviceButton : IDeviceInput { }

    public class DeviceButtonEventArgs : EventArgs
    {
        public DeviceButtonEventArgs(IDeviceButton button)
        {
            Button = button;
        }

        public IDeviceButton Button { get; }
    }
}