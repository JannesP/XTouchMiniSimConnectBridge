using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.Device
{
    public interface IDevice : IDisposable
    {
        string DeviceName { get; }

        string TechnicalDeviceIdentifier { get; }

        bool IsConnected { get; }

        event EventHandler? Connected;
        event EventHandler? Disconnected;

        Task<bool> ConnectAsync();
        Task DisconnectAsync();

        Task ResetDeviceState();

        IEnumerable<IDeviceButton> Buttons { get; }
        event EventHandler<DeviceButtonEventArgs>? ButtonDown;
        event EventHandler<DeviceButtonEventArgs>? ButtonUp;

        IEnumerable<IDeviceEncoder> Encoders { get; }
        event EventHandler<DeviceEncoderEventArgs>? EncoderTurned;

        IEnumerable<IDeviceFader> Faders { get; }
        event EventHandler<DeviceFaderEventArgs>? FaderMoved;

        IEnumerable<IDeviceLed> Leds { get; }
        Task SetLedState(IDeviceLed deviceLed, DeviceLedState ledState);
    }

    public interface IDeviceControl
    {
        int Id { get; }

        string Name { get; }
    }

    public interface IDeviceInput : IDeviceControl { }

    public interface IDeviceOutput : IDeviceControl { }
}
