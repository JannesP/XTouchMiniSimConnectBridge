using System;
using System.Collections.Generic;
using System.Text;

namespace JannesP.DeviceSimConnectBridge.Device
{
    public interface IDevice
    {
        IEnumerable<IDeviceButton> Buttons { get; }
        event EventHandler<IDeviceButtonEventArgs> ButtonDown;
        event EventHandler<IDeviceButtonEventArgs> ButtonUp;

        IEnumerable<IDeviceEncoder> Encoders { get; }
        event EventHandler<IDeviceEncoderEventArgs> EncoderTurned;

        IEnumerable<IDeviceFader> Faders { get; }
        event EventHandler<IDeviceFaderEventArgs> FaderTurned;

        IEnumerable<IDeviceLed> Leds { get; }
        void SetLedState(IDeviceLed deviceLed, DeviceLedState ledState);
    }

    public interface IDeviceControl
    {
        int Id { get; }

        string Name { get; }
    }

    public interface IDeviceInput : IDeviceControl { }

    public interface IDeviceOutput : IDeviceControl { }
}
