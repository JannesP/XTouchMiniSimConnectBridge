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

        enum State
        {
            Closed,
            Opening,
            Open,
            Closing,
            Disposed,
        }

        private readonly object _syncRoot = new object();
        private State _state = State.Closed;

        private Win32MidiInput? _midiIn;
        private Win32MidiOutput? _midiOut;

        public XTouchMiniMcMode() { }

        public event EventHandler<XTouchMiniMcModeButtonEventArgs>? ButtonDown;
        public event EventHandler<XTouchMiniMcModeButtonEventArgs>? ButtonUp;
        public event EventHandler<XTouchMiniMcModeEncoderTurnedEventArgs>? EncoderTurned;
        public event EventHandler<XTouchMiniMcModeFaderMovedEventArgs>? FaderMoved;

        public Task<bool> OpenDeviceAsync()
        {
            lock (_syncRoot)
            {
                if (_state != State.Closed)
                {
                    throw new InvalidOperationException($"OpenDevice is only possible if the state is {State.Closed}.");
                }
                _state = State.Opening;
            }
            return Task.Run<bool>(() =>
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
                    lock (_syncRoot)
                    {
                        UnsafeDispose();
                        _state = State.Closed;
                    }
                    throw;
                }
                lock (_syncRoot)
                {
                    if (_state == State.Disposed)
                    {
                        UnsafeDispose();
                    }
                    else
                    {
                        _state = State.Open;
                    }
                }
                try
                {
                    //set OpMode to Mackie Control (MC)
                    _midiOut.Send(MidiEventStatusType.CC, 0x7f, (byte)OpMode.MackieControl);
                    ResetState();
                }
                catch
                {
                    lock (_syncRoot)
                    {
                        UnsafeDispose();
                        _state = State.Closed;
                    }
                    throw;
                }
                return true;
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
                            ButtonDown?.Invoke(this, new XTouchMiniMcModeButtonEventArgs(button));
                        }
                        else if (e.MidiEvent.Arg2 == (byte)McButtonState.Up)
                        {
                            //button up
                            ButtonUp?.Invoke(this, new XTouchMiniMcModeButtonEventArgs(button));
                        }
                        else
                        {
                            Console.WriteLine("[_midiIn_MidiEventReceived] {0}", e.MidiEvent.ToString());
                        }
                    }
                    break;
                case MidiEventStatusType.CC:
                    if (EncoderTurned == null) return;
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
                        EncoderTurned?.Invoke(this, new XTouchMiniMcModeEncoderTurnedEventArgs(encoder, ticks));
                    }
                    break;
                case MidiEventStatusType.Pitch:
                    if (FaderMoved == null) return;
                    if (XTouchMiniMcFader.Controls.TryGetValue(e.MidiEvent.Channel, out XTouchMiniMcFader fader))
                    {
                        /*
                         * translate the native value range from 0-127 to a double 0-1
                         */
                        double value = ((double)e.MidiEvent.Arg2) / 127d;
                        FaderMoved?.Invoke(this, new XTouchMiniMcModeFaderMovedEventArgs(XTouchMiniMcFader.Encoder1, value));
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
                    if (_state != State.Open)
                    {
                        throw new InvalidOperationException($"CloseDevice is only possible if the state is {State.Open}.");
                    }
                    ResetState();
                    _state = State.Closing;

                    UnsafeDispose();
                    _state = State.Closed;
                }
            });
        }

        public void SetButtonLed(XTouchMiniMcButton button, McLedState ledState)
        {
            if (_state != State.Open)
            {
                throw new InvalidOperationException($"SetButtonLight is only possible if the state is {State.Open}.");
            }
            _midiOut?.Send(MidiEventStatusType.NoteOn, button.MidiCode, (byte)ledState);
        }

        public void SetEncoderLed(XTouchMiniMcEncoder encoder, McEncoderRingStyle ringStyle, byte value)
        {
            if (_state != State.Open)
            {
                throw new InvalidOperationException($"SetButtonLight is only possible if the state is {State.Open}.");
            }
            if (value > 0x0F) throw new ArgumentOutOfRangeException(nameof(value), value, "Maximum value for Encoder LEDs is 0xF");
            _midiOut?.Send(MidiEventStatusType.CC, encoder.LedMidiCode, (byte)((byte)ringStyle + value));
        }

        public void ResetState()
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

        public void Dispose()
        {
            lock (_syncRoot)
            {
                _state = State.Disposed;
                UnsafeDispose();
            }
        }
    }
}
