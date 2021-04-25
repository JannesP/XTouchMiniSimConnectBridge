using JannesP.Midi;
using JannesP.Midi.MidiProtocol;
using JannesP.Midi.Natives;
using JannesP.XTouchMini.Enums;
using JannesP.XTouchMini.EventArgs;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.XTouchMini
{
    public class XTouchMiniMcMode : IDisposable
    {
        public static string MidiDeviceName { get; } = "X-TOUCH MINI";

        public enum ConnectionState
        {
            Closed,
            Opening,
            Open,
            Closing,
            Disposed,
        }

        private readonly object _syncRoot = new object();
        private ConnectionState _state = ConnectionState.Closed;

        private Win32MidiInput? _midiIn;
        private Win32MidiOutput? _midiOut;

        public XTouchMiniMcMode() { }

        public event EventHandler<XTouchMiniMcModeButtonEventArgs>? ButtonDown;
        public event EventHandler<XTouchMiniMcModeButtonEventArgs>? ButtonUp;
        public event EventHandler<XTouchMiniMcModeEncoderTurnedEventArgs>? EncoderTurned;
        public event EventHandler<XTouchMiniMcModeFaderMovedEventArgs>? FaderMoved;
        public event EventHandler? Connected;
        public event EventHandler? Disconnected;

        protected virtual void OnButtonDown(XTouchMiniMcButton button) => ButtonDown?.Invoke(this, new XTouchMiniMcModeButtonEventArgs(button));
        protected virtual void OnButtonUp(XTouchMiniMcButton button) => ButtonUp?.Invoke(this, new XTouchMiniMcModeButtonEventArgs(button));
        protected virtual void OnEncoderTurned(XTouchMiniMcEncoder encoder, int ticks) => EncoderTurned?.Invoke(this, new XTouchMiniMcModeEncoderTurnedEventArgs(encoder, ticks));
        protected virtual void OnFaderMoved(XTouchMiniMcFader fader, double value) => FaderMoved?.Invoke(this, new XTouchMiniMcModeFaderMovedEventArgs(fader, value));
        protected virtual void OnConnected() => Connected?.Invoke(this, new System.EventArgs());
        protected virtual void OnDisconnected() => Disconnected?.Invoke(this, new System.EventArgs());

        public ConnectionState State => _state;

        public Task<bool> OpenDeviceAsync()
        {
            lock (_syncRoot)
            {
                if (_state != ConnectionState.Closed)
                {
                    throw new InvalidOperationException($"OpenDevice is only possible if the state is {ConnectionState.Closed}.");
                }
                _state = ConnectionState.Opening;
            }
            return Task.Run<bool>(() =>
            {
                lock (_syncRoot)
                {
                    uint? inputId = Win32MidiInput.FindInputIdByName(MidiDeviceName);
                    uint? outputId = null;
                    if (inputId != null)
                    {
                        outputId = Win32MidiOutput.FindOutputIdByName(MidiDeviceName);
                    }
                    if (inputId == null || outputId == null)
                    {
                        return false;
                    }
                    _midiIn = new Win32MidiInput(inputId.Value);
                    _midiIn.MidiEventReceived += MidiIn_MidiEventReceived;
                    try
                    {
                        _midiOut = new Win32MidiOutput(outputId.Value);
                    }
                    catch
                    {

                        UnsafeDispose();
                        _state = ConnectionState.Closed;

                        throw;
                    }
                    if (_state == ConnectionState.Disposed)
                    {
                        UnsafeDispose();
                    }
                    else
                    {
                        _state = ConnectionState.Open;
                    }
                    try
                    {
                        //set OpMode to Mackie Control (MC)
                        _midiOut.Send(MidiEventStatusType.CC, 0x7f, (byte)OpMode.MackieControl);
                        ResetDeviceState();
                        OnConnected();
                    }
                    catch
                    {
                        UnsafeDispose();
                        _state = ConnectionState.Closed;
                        throw;
                    }
                    return true;
                }
            });
        }

        private void MidiIn_MidiEventReceived(object sender, MidiEventReceivedEventArgs e)
        {
            switch (e.MidiEvent.Status)
            {
                case MidiEventStatusType.NoteOn:
                    if (XTouchMiniMcButton.Controls.TryGetValue(e.MidiEvent.Arg1, out XTouchMiniMcButton button)) {
                        if (e.MidiEvent.Arg2 == (byte)McButtonState.Down)
                        {
                            //button down
                            OnButtonDown(button);
                        }
                        else if (e.MidiEvent.Arg2 == (byte)McButtonState.Up)
                        {
                            //button up
                            OnButtonUp(button);
                        }
                        else
                        {
                            Console.WriteLine("[_midiIn_MidiEventReceived] {0}", e.MidiEvent.ToString());
                        }
                    }
                    break;
                case MidiEventStatusType.CC:
                    if (XTouchMiniMcEncoder.Controls.TryGetValue(e.MidiEvent.Arg1, out XTouchMiniMcEncoder encoder))
                    {
                        /*
                         * apparently the encoder works like this:
                         * Clockwise: 0 < Arg2 < 0x40
                         * Anti-Clockwise: 40 < Arg2 < ?? (presumably 0x80)
                         * 
                         * the value depends on the turn speed, no idea if it's just 
                         * reporting the amount of "steps" since the last measurement
                         * 
                         * I just transform it to an int
                         * negative: anti-clockwise
                         * positive: clockwise
                         * absolute value: speed (however that is measured)
                        */
                        int ticks;
                        if (e.MidiEvent.Arg2 > 0x40)
                        {
                            ticks = -(e.MidiEvent.Arg2 - 0x40);
                        }
                        else
                        {
                            ticks = e.MidiEvent.Arg2;
                        }
                        OnEncoderTurned(encoder, ticks);
                    }
                    break;
                case MidiEventStatusType.Pitch:
                    if (XTouchMiniMcFader.Controls.TryGetValue(e.MidiEvent.Channel, out XTouchMiniMcFader fader))
                    {
                        /*
                         * translate the native value range from 0-127 to a double 0-1
                         */
                        double value = ((double)e.MidiEvent.Arg2) / 127d;
                        OnFaderMoved(fader, value);
                    }
                    break;
                default:
                    Console.WriteLine("[_midiIn_MidiEventReceived] {0}", e.MidiEvent.ToString());
                    break;
            }
        }

        public Task CloseDeviceAsync()
        {
            return Task.Run(() =>
            {
                lock (_syncRoot)
                {
                    if (_state != ConnectionState.Open)
                    {
                        throw new InvalidOperationException($"CloseDevice is only possible if the state is {ConnectionState.Open}.");
                    }
                    ResetDeviceState();
                    _state = ConnectionState.Closing;

                    UnsafeDispose();
                    _state = ConnectionState.Closed;
                }
                OnDisconnected();
            });
        }

        public void SetButtonLed(XTouchMiniMcButton button, McLedState ledState)
        {
            if (_state != ConnectionState.Open)
            {
                throw new InvalidOperationException($"SetButtonLed is only possible if the state is {ConnectionState.Open}.");
            }
            _midiOut?.Send(MidiEventStatusType.NoteOn, button.MidiCode, (byte)ledState);
        }

        public void SetEncoderLed(XTouchMiniMcEncoder encoder, McEncoderRingStyle ringStyle, byte value)
        {
            if (_state != ConnectionState.Open)
            {
                throw new InvalidOperationException($"SetEncoderLed is only possible if the state is {ConnectionState.Open}.");
            }
            if (value > 0x0F) throw new ArgumentOutOfRangeException(nameof(value), value, "Maximum value for Encoder LEDs is 0xF");
            _midiOut?.Send(MidiEventStatusType.CC, encoder.LedMidiCode, (byte)((byte)ringStyle + value));
        }

        public void ResetDeviceState()
        {
            foreach (var entry in XTouchMiniMcButton.Controls) SetButtonLed(entry.Value, McLedState.Off);
            foreach (var entry in XTouchMiniMcEncoder.Controls) SetEncoderLed(entry.Value, McEncoderRingStyle.Fan, 0x0);       
        }

        private void UnsafeDispose()
        {
            _midiIn?.Dispose();
            _midiIn = null;
            _midiOut?.Dispose();
            _midiOut = null;
        }

        public virtual void Dispose()
        {
            lock (_syncRoot)
            {
                _state = ConnectionState.Disposed;
                UnsafeDispose();
            }
        }
    }
}
