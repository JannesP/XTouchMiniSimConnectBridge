using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.Device
{
    public interface IDevice : IDisposable
    {
        event EventHandler<DeviceButtonEventArgs>? ButtonDown;

        event EventHandler<DeviceButtonEventArgs>? ButtonUp;

        event EventHandler? Connected;

        event EventHandler? Disconnected;

        event EventHandler<DeviceEncoderEventArgs>? EncoderTurned;

        event EventHandler<DeviceFaderEventArgs>? FaderMoved;

        IEnumerable<IDeviceButton> Buttons { get; }
        string? DeviceId { get; }
        string DeviceType { get; }
        IEnumerable<IDeviceEncoder> Encoders { get; }
        IEnumerable<IDeviceFader> Faders { get; }
        string FriendlyName { get; }
        bool IsConnected { get; }
        IEnumerable<IDeviceLed> Leds { get; }

        Task<bool> ConnectAsync();

        Task DisconnectAsync();

        Task ResetDeviceState();

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