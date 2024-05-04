namespace JannesP.XTouchMini
{
    public class XTouchMiniMcEncoder : ControlDefinition<XTouchMiniMcEncoder>
    {
        public static readonly XTouchMiniMcEncoder Encoder1 = new(_baseControl + 0x00, _baseLed + 0x00, "Encoder 1");
        public static readonly XTouchMiniMcEncoder Encoder2 = new(_baseControl + 0x01, _baseLed + 0x01, "Encoder 2");
        public static readonly XTouchMiniMcEncoder Encoder3 = new(_baseControl + 0x02, _baseLed + 0x02, "Encoder 3");
        public static readonly XTouchMiniMcEncoder Encoder4 = new(_baseControl + 0x03, _baseLed + 0x03, "Encoder 4");
        public static readonly XTouchMiniMcEncoder Encoder5 = new(_baseControl + 0x04, _baseLed + 0x04, "Encoder 5");
        public static readonly XTouchMiniMcEncoder Encoder6 = new(_baseControl + 0x05, _baseLed + 0x05, "Encoder 6");
        public static readonly XTouchMiniMcEncoder Encoder7 = new(_baseControl + 0x06, _baseLed + 0x06, "Encoder 7");
        public static readonly XTouchMiniMcEncoder Encoder8 = new(_baseControl + 0x07, _baseLed + 0x07, "Encoder 8");
        private const int _baseControl = 0x10;
        private const int _baseLed = 0x30;

        private XTouchMiniMcEncoder(byte buttonMidiCode, byte ledMidiCode, string name) : base(buttonMidiCode, name)
        {
            LedMidiCode = ledMidiCode;
        }

        public byte LedMidiCode { get; }
    }
}