using System;

namespace JannesP.DeviceSimConnectBridge.Device
{
    [Flags]
    public enum DeviceLedState
    {
        Off = 1,
        On = 2,
        Blinking = 4,
    }

    public interface IDeviceLed : IDeviceOutput
    {
        DeviceLedState ValidStates { get; }
    }
}