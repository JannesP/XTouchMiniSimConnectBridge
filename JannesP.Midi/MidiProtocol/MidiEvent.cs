using System;
using System.Collections.Generic;
using System.Text;

namespace JannesP.Midi.MidiProtocol
{
    public class MidiEventLongData : MidiEventBase
    {
        public MidiEventLongData(byte status, byte[]? extraData, int extraDataOffset, int extraDataLength, long? timestamp = null) : base(status, timestamp)
        {
            ExtraData = extraData;
            ExtraDataOffset = extraDataOffset;
            ExtraDataLength = extraDataLength;
        }

        public readonly byte[]? ExtraData;
        public readonly int ExtraDataOffset;
        public readonly int ExtraDataLength;

        public override string ToString()
        {
            return string.Format("{0:X02}:{1}", StatusByte, ExtraData != null ? "[data:" + ExtraDataLength + "]" : "");
        }
    }

    public class MidiEvent : MidiEventBase
    {
        public MidiEvent(byte status, byte arg1, byte arg2, long? timestamp = null) : base(status, timestamp)
        {
            Arg1 = arg1;
            Arg2 = arg2;
        }
        public MidiEvent(MidiEventStatusType status, byte arg1, byte arg2, long? timestamp = null) : this((byte)status, arg1, arg2, timestamp) { }

        public byte Arg1 { get; }
        public byte Arg2 { get; }

        public uint Message
        {
            get
            {
                return (uint)(StatusByte | (Arg1 << 8) | (Arg2 << 16));
            }
        }

        public override string ToString()
        {
            string channel = StatusByte < 0xF0 ? Channel.ToString() : "X";
            return string.Format("{0:X02} ({1}:{2}) - {3:X02}:{4:X02}", StatusByte, Status, channel, Arg1, Arg2);
        }
    }

    public abstract class MidiEventBase
    {
        public MidiEventBase(byte status, long? timestamp) 
        {
            StatusByte = status;
            Timestamp = timestamp;
        }

        public byte StatusByte { get; }
        public long? Timestamp { get; }

        public byte Channel
        {
            get
            {
                if (StatusByte >= 0xF0) throw new InvalidOperationException("Midi status bytes starting with 0xF0 don't have a channel. If you really need the data use StatusByte to get the raw byte.");
                return (byte)(StatusByte & 0x0F);
            }
        }

        public MidiEventStatusType Status
        {
            get
            {
                if (StatusByte < 0xF0)
                {
                    return (MidiEventStatusType)(StatusByte & 0xF0);
                }
                return (MidiEventStatusType)StatusByte;
            }
        }

        public static byte FixedDataSize(MidiEventStatusType status) => FixedDataSize((byte)status);
        public static byte FixedDataSize(byte statusByte)
        {
            switch ((MidiEventStatusType)(statusByte & 0xF0))
            {
                case MidiEventStatusType.SysEx1: // and 0xF7, 0xFF
                    switch ((MidiEventStatusType)statusByte)
                    {
                        case MidiEventStatusType.MtcQuarterFrame:
                        case MidiEventStatusType.SongSelect:
                            return 1;
                        case MidiEventStatusType.SongPositionPointer:
                            return 2;
                        default:
                            return 0; // no fixed data
                    }
                case MidiEventStatusType.Program:
                case MidiEventStatusType.CAf:
                    return 1;
                default:
                    return 2;
            }
        }
    }

    public enum MidiEventStatusType : byte
    {
        NoteOff = 0x80,
        NoteOn = 0x90,
        PAf = 0xA0,
        CC = 0xB0,
        Program = 0xC0,
        CAf = 0xD0,
        Pitch = 0xE0,
        SysEx1 = 0xF0,
        MtcQuarterFrame = 0xF1,
        SongPositionPointer = 0xF2,
        SongSelect = 0xF3,
        TuneRequest = 0xF6,
        SysEx2 = 0xF7,
        MidiClock = 0xF8,
        MidiTick = 0xF9,
        MidiStart = 0xFA,
        MidiContinue = 0xFB,
        MidiStop = 0xFC,
        //0xFD is Undefined (Reserved)
        ActiveSense = 0xFE,
        Reset = 0xFF,
    }
}
