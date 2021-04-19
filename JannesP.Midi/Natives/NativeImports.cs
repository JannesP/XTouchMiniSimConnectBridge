using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace JannesP.Midi.Natives
{

    #region Type Definitions

    [StructLayout(LayoutKind.Sequential)]
    public struct MIDIINCAPS
    {
        public ushort wMid;
        public ushort wPid;
        public uint vDriverVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NativeImports.MAXPNAMELEN)]
        public string szPname;
        public uint dwSupport;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIDIOUTCAPS
    {
        public ushort wMid;
        public ushort wPid;
        public uint vDriverVersion;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int)NativeImports.MAXPNAMELEN)]
        public string szPname;
        public MidiOutputTechnology wTechnology;
        public ushort wVoices;
        public ushort wNotes;
        public ushort wChannelMask;
        public MidiOptionalFunctionality dwSupport;
    }

    public enum MidiOutputTechnology : ushort
    {
        MOD_MIDIPORT = 1,
        MOD_SYNTH = 2,
        MOD_SQSYNTH = 3,
        MOD_FMSYNTH = 4,
        MOD_MAPPER = 5,
        MOD_WAVETABLE = 6,
        MOD_SWSYNTH = 7,
    }

    [Flags]
    public enum MidiOptionalFunctionality : uint
    {
        MIDICAPS_VOLUME = 0x0001,
        MIDICAPS_LRVOLUME = 0x0002,
        MIDICAPS_CACHE = 0x0004,
        MIDICAPS_STREAM = 0x0008,
    }

    [Flags]
    public enum MidiCallbackFlags : uint
    {
        CALLBACK_TYPEMASK = 0x70000,
        CALLBACK_NULL = 0x00000,
        CALLBACK_WINDOW = 0x10000,
        CALLBACK_TASK = 0x20000,
        CALLBACK_FUNCTION = 0x30000,
        CALLBACK_THREAD = CALLBACK_TASK,
        CALLBACK_EVENT = 0x50000,
        MIDI_IO_STATUS = 0x00020,
    }

    public enum MidiOutMessage : uint
    {
        MOM_OPEN = 0x3C7,
        MOM_CLOSE = 0x3C8,
        MOM_DONE = 0x3C9
    }

    public enum MidiInMessage : uint
    {
        MIM_OPEN = 0x3C1,
        MIM_CLOSE = 0x3C2,
        MIM_DATA = 0x3C3,
        MIM_LONGDATA = 0x3C4,
        MIM_ERROR = 0x3C5,
        MIM_LONGERROR = 0x3C6,
        MIM_MOREDATA = 0x3CC
    }

    public enum MMRESULT : uint
    {
        MMSYSERR_NOERROR = 0,
        MMSYSERR_ERROR = 1,
        MMSYSERR_BADDEVICEID = 2,
        MMSYSERR_NOTENABLED = 3,
        MMSYSERR_ALLOCATED = 4,
        MMSYSERR_INVALHANDLE = 5,
        MMSYSERR_NODRIVER = 6,
        MMSYSERR_NOMEM = 7,
        MMSYSERR_NOTSUPPORTED = 8,
        MMSYSERR_BADERRNUM = 9,
        MMSYSERR_INVALFLAG = 10,
        MMSYSERR_INVALPARAM = 11,
        MMSYSERR_HANDLEBUSY = 12,
        MMSYSERR_INVALIDALIAS = 13,
        MMSYSERR_BADDB = 14,
        MMSYSERR_KEYNOTFOUND = 15,
        MMSYSERR_READERROR = 16,
        MMSYSERR_WRITEERROR = 17,
        MMSYSERR_DELETEERROR = 18,
        MMSYSERR_VALNOTFOUND = 19,
        MMSYSERR_NODRIVERCB = 20,
        WAVERR_BADFORMAT = 32,
        WAVERR_STILLPLAYING = 33,
        WAVERR_UNPREPARED = 34,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HMIDIOUT
    {
        public IntPtr handle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct HMIDIIN
    {
        public IntPtr handle;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MIDIHDR
    {
        public IntPtr lpData;
        public uint dwBufferLength;
        public uint dwBytesRecorded;
        public IntPtr dwUser;
        public uint dwFlags;
        public IntPtr lpNext;     //MIDIHDR
        public IntPtr reserved;
        public uint dwOffset;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public IntPtr[] dwReserved;
    }

    #endregion Type Definitions

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Keeping names of the actual Win32 API calls.")]
    public static class NativeImports
    {
        public static void ThrowOnError(MMRESULT errorCode)
        {
            Marshal.ThrowExceptionForHR((int)errorCode);
        }

        public const uint MAXPNAMELEN = 32;

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint midiInGetNumDevs();
        [DllImport("winmm.dll", SetLastError = true)]
        public static extern uint midiOutGetNumDevs();

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern MMRESULT midiInGetDevCaps(IntPtr uDeviceID, ref MIDIINCAPS caps, uint cbMidiInCaps);
        public static MMRESULT midiInGetDevCaps(uint deviceId, ref MIDIINCAPS caps)
            => midiInGetDevCaps(new IntPtr(deviceId), ref caps, (uint)Marshal.SizeOf<MIDIINCAPS>());

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern MMRESULT midiOutGetDevCaps(IntPtr uDeviceID, ref MIDIOUTCAPS lpMidiOutCaps, uint cbMidiOutCaps);
        public static MMRESULT midiOutGetDevCaps(uint deviceId, ref MIDIOUTCAPS caps)
            => midiOutGetDevCaps(new IntPtr(deviceId), ref caps, (uint)Marshal.SizeOf<MIDIOUTCAPS>());

        public delegate void MidiInProc(HMIDIIN hMidiIn, MidiInMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);
        [DllImport("winmm.dll", SetLastError = true)]
        private static extern MMRESULT midiInOpen(out HMIDIIN lphMidiIn, uint uDeviceID, MidiInProc dwCallback, IntPtr dwInstance, MidiCallbackFlags dwFlags);
        public static MMRESULT midiInOpen(out HMIDIIN lphMidiIn, uint uDeviceID, MidiInProc dwCallback, IntPtr dwInstance)
            => midiInOpen(out lphMidiIn, uDeviceID, dwCallback, dwInstance, dwCallback == null ? MidiCallbackFlags.CALLBACK_NULL : MidiCallbackFlags.CALLBACK_FUNCTION);
        
        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiInClose(HMIDIIN hMidiIn);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiInStart(HMIDIIN hMidiIn);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiInStop(HMIDIIN hMidiIn);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiInReset(HMIDIIN hMidiIn);

        public delegate void MidiOutProc(HMIDIOUT hmo, MidiOutMessage wMsg, IntPtr dwInstance, IntPtr dwParam1, IntPtr dwParam2);
        [DllImport("winmm.dll")]
        private static extern MMRESULT midiOutOpen(out HMIDIOUT lphMidiOut, uint uDeviceID, MidiOutProc? dwCallback, IntPtr dwInstance, MidiCallbackFlags dwFlags);
        public static MMRESULT midiOutOpen(out HMIDIOUT lphMidiOut, uint uDeviceID, MidiOutProc? dwCallback, IntPtr dwInstance)
            => midiOutOpen(out lphMidiOut, uDeviceID, dwCallback, dwInstance, dwCallback == null ? MidiCallbackFlags.CALLBACK_NULL : MidiCallbackFlags.CALLBACK_FUNCTION);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiOutClose(HMIDIOUT hMidiOut);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiOutReset(HMIDIOUT hMidiOut);

        [DllImport("winmm.dll", SetLastError = true)]
        public static extern MMRESULT midiOutShortMsg(HMIDIOUT hmo, uint dwMsg);

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern MMRESULT midiInPrepareHeader(HMIDIIN hmi, IntPtr pmh, uint cbmh);
        public static MMRESULT midiInPrepareHeader(HMIDIIN hmi, IntPtr pmh)
            => midiInPrepareHeader(hmi, pmh, (uint)Marshal.SizeOf<MIDIHDR>());

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern MMRESULT midiInUnprepareHeader(HMIDIIN hmi, IntPtr pmh, uint cbmh);
        public static MMRESULT midiInUnprepareHeader(HMIDIIN hmi, IntPtr pmh)
            => midiInPrepareHeader(hmi, pmh, (uint)Marshal.SizeOf<MIDIHDR>());

        [DllImport("winmm.dll", SetLastError = true)]
        private static extern MMRESULT midiInAddBuffer(HMIDIIN hmi, IntPtr pmh, uint cbmh);
        public static MMRESULT midiInAddBuffer(HMIDIIN hmi, IntPtr pmh)
            => midiInPrepareHeader(hmi, pmh, (uint)Marshal.SizeOf<MIDIHDR>());


    }
}
