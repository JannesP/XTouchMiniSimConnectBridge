using System;

namespace JannesP.DeviceSimConnectBridge.Device
{
    public interface IDeviceLed : IDeviceOutput 
    { 
        DeviceLedState ValidStates { get; }
    }

    [Flags]
    public enum DeviceLedState
    {
        Off = 1,
        On = 2,
        Blinking = 4,
    }
}
