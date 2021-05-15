using System;
using JannesP.Midi.MidiProtocol;
using JannesP.Midi.Natives;

namespace JannesP.Midi
{
    public class Win32MidiOutput : IDisposable
    {
        private readonly HMIDIOUT _midiHandle;
        private bool _disposedValue;

        public Win32MidiOutput(uint deviceId)
        {
            NativeImports.ThrowOnError(NativeImports.midiOutOpen(out _midiHandle, deviceId, null, IntPtr.Zero));
        }

        ~Win32MidiOutput()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public static uint? FindOutputIdByName(string name)
        {
            uint midiDevNum = NativeImports.midiOutGetNumDevs();
            for (uint i = 0; i < midiDevNum; i++)
            {
                MIDIOUTCAPS capsResult = default;
                NativeImports.ThrowOnError(NativeImports.midiOutGetDevCaps(i, ref capsResult));
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

        public void Send(MidiEventStatusType status, byte arg1, byte arg2)
                                    => Send(new MidiEvent(status, arg1, arg2));

        public void Send(MidiEvent evt)
        {
            NativeImports.ThrowOnError(NativeImports.midiOutShortMsg(_midiHandle, evt.Message));
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposedValue)
            {
                if (disposing)
                {
                    // nothing here
                }
                NativeImports.midiOutReset(_midiHandle);
                NativeImports.midiOutClose(_midiHandle);
                _disposedValue = true;
            }
        }
    }
}