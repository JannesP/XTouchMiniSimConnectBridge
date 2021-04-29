using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTimeModels
{
    public class DesignTimeDevice : IDevice
    {
        public static readonly string DesignTimeTechnicalDeviceIdentifier = "design_time_device";

        public string DeviceName => "DesignTime Device";
        public string TechnicalDeviceIdentifier => DesignTimeTechnicalDeviceIdentifier;
        public bool IsConnected => true;

        public IEnumerable<IDeviceButton> Buttons { get; } = new List<IDeviceButton>();
        public IEnumerable<IDeviceEncoder> Encoders { get; } = new List<IDeviceEncoder>();
        public IEnumerable<IDeviceFader> Faders { get; } = new List<IDeviceFader>();
        public IEnumerable<IDeviceLed> Leds { get; } = new List<IDeviceLed>();

        public event EventHandler? Connected;
        public event EventHandler? Disconnected;
        public event EventHandler<DeviceButtonEventArgs>? ButtonDown;
        public event EventHandler<DeviceButtonEventArgs>? ButtonUp;
        public event EventHandler<DeviceEncoderEventArgs>? EncoderTurned;
        public event EventHandler<DeviceFaderEventArgs>? FaderMoved;

        public Task<bool> ConnectAsync() => throw new NotSupportedException();
        public Task DisconnectAsync() => throw new NotSupportedException();
        public Task ResetDeviceState() => throw new NotSupportedException();
        public Task SetLedState(IDeviceLed deviceLed, DeviceLedState ledState) => throw new NotSupportedException();
        public void Dispose() { }
    }
}
