using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.XTouchMini;
using JannesP.XTouchMini.Enums;

namespace JannesP.DeviceSimConnectBridge.Device.XTouchMini
{
    public class XTouchMiniDevice : XTouchMiniMcMode, IDevice
    {
        public XTouchMiniDevice() : base() { }

        private readonly Dictionary<byte, IDeviceButton> _buttons = XTouchMiniMcButton.Controls.Select(v => new DeviceButton((int)v.Value.MidiCode, v.Value.Name)).Cast<IDeviceButton>().ToDictionary(v => (byte)v.Id);
        public IEnumerable<IDeviceButton> Buttons => _buttons.Values;

        private readonly Dictionary<byte, IDeviceEncoder> _encoders = XTouchMiniMcEncoder.Controls.Select(v => new DeviceEncoder((int)v.Value.MidiCode, v.Value.Name)).Cast<IDeviceEncoder>().ToDictionary(v => (byte)v.Id);
        public IEnumerable<IDeviceEncoder> Encoders => _encoders.Values;

        private readonly Dictionary<byte, IDeviceFader> _faders = XTouchMiniMcFader.Controls.Select(v => new DeviceFader((int)v.Value.MidiCode, v.Value.Name)).Cast<IDeviceFader>().ToDictionary(v => (byte)v.Id);
        public IEnumerable<IDeviceFader> Faders => _faders.Values;

        private readonly Dictionary<byte, IDeviceLed> _leds = XTouchMiniMcButton.Controls.Select(v => new DeviceButton((int)v.Value.MidiCode, v.Value.Name)).Cast<IDeviceLed>().ToDictionary(v => (byte)v.Id);
        public IEnumerable<IDeviceLed> Leds => _leds.Values;

        public bool IsConnected => State == ConnectionState.Open;

        public string DeviceName => "Behringer X-Touch Mini";

        public string TechnicalDeviceIdentifier => "behringer_xtouch_mini";

        public new event EventHandler? Connected;
        public new event EventHandler? Disconnected;
        public new event EventHandler<DeviceButtonEventArgs>? ButtonDown;
        public new event EventHandler<DeviceButtonEventArgs>? ButtonUp;
        public new event EventHandler<DeviceEncoderEventArgs>? EncoderTurned;
        public new event EventHandler<DeviceFaderEventArgs>? FaderMoved;

        public Task<bool> ConnectAsync() => base.OpenDeviceAsync();
        public Task DisconnectAsync() => base.CloseDeviceAsync();
        public void SetLedState(IDeviceLed deviceLed, DeviceLedState ledState)
        {
            McLedState xTouchLedState = ledState switch
            {
                DeviceLedState.Off => McLedState.Off,
                DeviceLedState.On => McLedState.On,
                DeviceLedState.Blinking => McLedState.Blinking,
                _ => throw new NotSupportedException("The given led state is not supported by this device."),
            };
            base.SetButtonLed(XTouchMiniMcButton.Controls[(byte)deviceLed.Id], xTouchLedState);
        }

        protected override void OnButtonDown(XTouchMiniMcButton button)
        {
            base.OnButtonDown(button);
            ButtonDown?.Invoke(this, new DeviceButtonEventArgs(_buttons[button.MidiCode]));
        }

        protected override void OnButtonUp(XTouchMiniMcButton button)
        {
            base.OnButtonUp(button);
            ButtonUp?.Invoke(this, new DeviceButtonEventArgs(_buttons[button.MidiCode]));
        }

        protected override void OnConnected()
        {
            base.OnConnected();
            Connected?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnDisconnected()
        {
            base.OnDisconnected();
            Disconnected?.Invoke(this, EventArgs.Empty);
        }

        protected override void OnEncoderTurned(XTouchMiniMcEncoder encoder, int ticks)
        {
            base.OnEncoderTurned(encoder, ticks);
            EncoderTurned?.Invoke(this, new DeviceEncoderEventArgs(_encoders[encoder.MidiCode], ticks));
        }

        protected override void OnFaderMoved(XTouchMiniMcFader fader, double value)
        {
            base.OnFaderMoved(fader, value);
            FaderMoved?.Invoke(this, new DeviceFaderEventArgs(_faders[fader.MidiCode], value));
        }

        public new Task ResetDeviceState() 
        {
            return Task.Run(() =>
            {
                base.ResetDeviceState();
            });
        }
        Task IDevice.SetLedState(IDeviceLed deviceLed, DeviceLedState ledState) => throw new NotImplementedException();
    }
}
