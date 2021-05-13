using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTime
{
    public class DesignTimeDevice : IDevice
    {
        public static readonly string DesignTimeDeviceType = "design_time_device_type";

        public string FriendlyName => "DesignTime Device";
        public string DeviceType => DesignTimeDeviceType;
        public string? DeviceId => null;
        public bool IsConnected => true;

        public IEnumerable<IDeviceButton> Buttons { get; } = new List<IDeviceButton>();
        public IEnumerable<IDeviceEncoder> Encoders { get; } = new List<IDeviceEncoder>();
        public IEnumerable<IDeviceFader> Faders { get; } = new List<IDeviceFader>();
        public IEnumerable<IDeviceLed> Leds { get; } = new List<IDeviceLed>();

        

#pragma warning disable CS0067
        public event EventHandler? Connected;
        public event EventHandler? Disconnected;
        public event EventHandler<DeviceButtonEventArgs>? ButtonDown;
        public event EventHandler<DeviceButtonEventArgs>? ButtonUp;
        public event EventHandler<DeviceEncoderEventArgs>? EncoderTurned;
        public event EventHandler<DeviceFaderEventArgs>? FaderMoved;
#pragma warning restore CS0067

        public Task<bool> ConnectAsync() => throw new NotSupportedException();
        public Task DisconnectAsync() => throw new NotSupportedException();
        public Task ResetDeviceState() => throw new NotSupportedException();
        public Task SetLedState(IDeviceLed deviceLed, DeviceLedState ledState) => throw new NotSupportedException();
        public void Dispose() => GC.SuppressFinalize(this);
    }
}
