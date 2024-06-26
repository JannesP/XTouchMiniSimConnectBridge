﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using JannesP.Midi.MidiProtocol;
using JannesP.Midi.Natives;

namespace JannesP.Midi
{
    public class MidiEventLongDataReceivedEventArgs : EventArgs
    {
        public MidiEventLongDataReceivedEventArgs(MidiEventLongData midiEvent)
        {
            MidiEvent = midiEvent;
        }

        public MidiEventLongData MidiEvent { get; set; }
    }

    public class MidiEventReceivedEventArgs : EventArgs
    {
        public MidiEventReceivedEventArgs(MidiEvent midiEvent)
        {
            MidiEvent = midiEvent;
        }

        public MidiEvent MidiEvent { get; set; }
    }

    public class Win32MidiInput : IDisposable
    {
        private readonly Dictionary<IntPtr, MidiInputLongDataBuffer> _longMessageBuffers = new Dictionary<IntPtr, MidiInputLongDataBuffer>();
        private readonly HMIDIIN _midiHandle;
        private readonly NativeImports.MidiInProc _midiInProc;
        private readonly object _syncRoot = new object();
        private bool _disposedValue;

        public Win32MidiInput(uint id)
        {
            lock (_syncRoot)
            {
                _midiInProc = new NativeImports.MidiInProc(MidiInProc);
                NativeImports.ThrowOnError(NativeImports.midiInOpen(out _midiHandle, id, _midiInProc, IntPtr.Zero));
                NativeImports.ThrowOnError(NativeImports.midiInStart(_midiHandle));
                while (_longMessageBuffers.Count < LongMessageBufferCount)
                {
                    var b = new MidiInputLongDataBuffer(_midiHandle);
                    b.PrepareHeader();
                    b.AddBuffer();
                    _longMessageBuffers.Add(b.Ptr, b);
                }
            }
        }

        ~Win32MidiInput()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public event EventHandler<MidiEventLongDataReceivedEventArgs>? MidiEventLongDataReceived;

        public event EventHandler<MidiEventReceivedEventArgs>? MidiEventReceived;

        public bool IsClosed { get; private set; }
        private int LongMessageBufferCount { get; } = 16;

        public static uint? FindInputIdByName(string name)
        {
            uint midiDevNum = NativeImports.midiInGetNumDevs();
            for (uint i = 0; i < midiDevNum; i++)
            {
                MIDIINCAPS capsResult = default;
                NativeImports.ThrowOnError(NativeImports.midiInGetDevCaps(i, ref capsResult));
                if (capsResult.szPname == name)
                {
                    return i;
                }
            }
            return null;
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                _disposedValue = true;
                IsClosed = true;
                if (disposing)
                {
                    //no managed resources to dispose yet :(
                }
                //silently ignore errors, what are we gonna do anyways?
                NativeImports.midiInReset(_midiHandle);
                NativeImports.midiInStop(_midiHandle);
                NativeImports.midiInClose(_midiHandle);

                foreach (KeyValuePair<IntPtr, MidiInputLongDataBuffer> kvp in _longMessageBuffers)
                {
                    kvp.Value.Dispose();
                }
                _longMessageBuffers.Clear();
            }
        }

        private void HandleLongData(IntPtr wParam1, IntPtr wParam2)
        {
            if (MidiEventLongDataReceived == null) return;
            byte[]? data = null;
            byte status;

            lock (_syncRoot)
            {
                MidiInputLongDataBuffer buffer = _longMessageBuffers[wParam1];
                // FIXME: this is a nasty workaround for https://github.com/atsushieno/managed-midi/issues/49
                // We have no idea when/how this message is sent (midi in proc is not well documented).
                if (buffer.Header.dwBytesRecorded == 0)
                    return;

                status = Marshal.ReadByte(buffer.Header.lpData);
                if (buffer.Header.dwBytesRecorded > 1)
                {
                    data = new byte[buffer.Header.dwBytesRecorded - 1];
                    Marshal.Copy(buffer.Header.lpData, data, 1, ((int)buffer.Header.dwBytesRecorded) - 1);
                }

                if (!IsClosed)
                {
                    buffer.Recycle();
                }
                else
                {
                    _longMessageBuffers.Remove(buffer.Ptr);
                    buffer.Dispose();
                }
            }

            MidiEventLongData e;
            if (data == null)
            {
                e = new MidiEventLongData(status, null, 0, 0, (long)wParam2);
            }
            else
            {
                e = new MidiEventLongData(status, data, 0, data.Length, (long)wParam2);
            }
            MidiEventLongDataReceived?.Invoke(this, new MidiEventLongDataReceivedEventArgs(e));
        }

        private void HandleMidiData(IntPtr dwParam1, IntPtr dwParam2)
        {
            if (MidiEventReceived == null) return;
            byte status = (byte)((uint)dwParam1 & 0xFF);
            byte msb = (byte)(((uint)dwParam1 & 0xFF00) >> 8);
            byte lsb = (byte)(((uint)dwParam1 & 0xFF0000) >> 16);
            byte size = MidiEvent.FixedDataSize(status);
            MidiEventReceived?.Invoke(this, new MidiEventReceivedEventArgs(new MidiEvent(status, msb, lsb, (long)dwParam2)));
        }

        private void MidiInProc(HMIDIIN hMidiIn, MidiInMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2)
        {
            switch (wMsg)
            {
                case MidiInMessage.MIM_DATA:
                    HandleMidiData(dwParam1, dwParam2);
                    break;

                case MidiInMessage.MIM_LONGDATA:
                    HandleLongData(dwParam1, dwParam2);
                    break;

                case MidiInMessage.MIM_MOREDATA:
                    // doesn't happen, since we don't use the IO_STATUS flag
                    break;

                case MidiInMessage.MIM_ERROR:
                    throw new InvalidOperationException($"Invalid MIDI message: {dwParam1}");
                case MidiInMessage.MIM_LONGERROR:
                    throw new InvalidOperationException("Invalid SysEx message.");
                default:
                    break;
            }
        }

        private class MidiInputLongDataBuffer : IDisposable
        {
            private readonly object _syncRoot = new object();
            private MIDIHDR? _header;
            private HMIDIIN _inputHandle;
            private bool _prepared = false;

            public MidiInputLongDataBuffer(HMIDIIN inputHandle, int bufferSize = 4096)
            {
                _inputHandle = inputHandle;

                var header = new MIDIHDR()
                {
                    lpData = Marshal.AllocHGlobal(bufferSize),
                    dwBufferLength = (uint)bufferSize,
                };

                try
                {
                    Ptr = Marshal.AllocHGlobal(Marshal.SizeOf<MIDIHDR>());
                    Marshal.StructureToPtr(header, Ptr, false);
                }
                catch
                {
                    Free();
                    throw;
                }
            }

            ~MidiInputLongDataBuffer()
            {
                if (_prepared)
                {
                    if (Header.lpData != IntPtr.Zero)
                    {
                        Marshal.FreeHGlobal(Header.lpData);
                    }
                }
                if (Ptr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(Ptr);
                }
            }

            public MIDIHDR Header
            {
                get
                {
                    lock (_syncRoot)
                    {
                        if (!_prepared) throw new Exception("The buffer isn't prepared, no read allowed!");
                        if (_header == null)
                        {
                            _header = Marshal.PtrToStructure<MIDIHDR>(Ptr);
                        }
                        return _header.Value;
                    }
                }
            }

            public IntPtr Ptr { get; set; } = IntPtr.Zero;

            public void AddBuffer() =>
                NativeImports.ThrowOnError(NativeImports.midiInAddBuffer(_inputHandle, Ptr));

            public void Dispose()
            {
                Free();
            }

            public void PrepareHeader()
            {
                lock (_syncRoot)
                {
                    _header = null;
                    if (!_prepared)
                        NativeImports.ThrowOnError(NativeImports.midiInPrepareHeader(_inputHandle, Ptr));
                    _prepared = true;
                }
            }

            public void Recycle()
            {
                UnPrepareHeader();
                PrepareHeader();
                AddBuffer();
            }

            public void UnPrepareHeader()
            {
                lock (_syncRoot)
                {
                    _header = null;
                    if (_prepared)
                        NativeImports.ThrowOnError(NativeImports.midiInUnprepareHeader(_inputHandle, Ptr));
                    _prepared = false;
                }
            }

            private void Free()
            {
                UnPrepareHeader();
            }
        }
    }
}