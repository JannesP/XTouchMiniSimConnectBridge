using System;

namespace JannesP.DeviceSimConnectBridge.Device
{
    public interface IDeviceFader : IDeviceInput { }

    public class DeviceFaderEventArgs : EventArgs
    {
        public DeviceFaderEventArgs(IDeviceFader fader, double value)
        {
            Fader = fader;
            Value = value;
        }

        public IDeviceFader Fader { get; }
        /// <summary>
        /// Value from 0 to 1
        /// </summary>
        public double Value { get; }

    }
}
