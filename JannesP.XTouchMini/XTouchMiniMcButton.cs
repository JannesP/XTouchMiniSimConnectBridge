namespace JannesP.XTouchMini
{
    public class XTouchMiniMcButton : ControlDefinition<XTouchMiniMcButton>
    {
        public static readonly XTouchMiniMcButton ButtonBottom1 = new(0x57, "Button Bottom 1");
        public static readonly XTouchMiniMcButton ButtonBottom2 = new(0x58, "Button Bottom 2");
        public static readonly XTouchMiniMcButton ButtonBottom3 = new(0x5B, "Button Bottom 3");
        public static readonly XTouchMiniMcButton ButtonBottom4 = new(0x5C, "Button Bottom 4");
        public static readonly XTouchMiniMcButton ButtonBottom5 = new(0x56, "Button Bottom 5");
        public static readonly XTouchMiniMcButton ButtonBottom6 = new(0x5D, "Button Bottom 6");
        public static readonly XTouchMiniMcButton ButtonBottom7 = new(0x5E, "Button Bottom 7");
        public static readonly XTouchMiniMcButton ButtonBottom8 = new(0x5F, "Button Bottom 8");

        public static readonly XTouchMiniMcButton ButtonLayerA = new(0x54, "Button Layer A");
        public static readonly XTouchMiniMcButton ButtonLayerB = new(0x55, "Button Layer B");

        public static readonly XTouchMiniMcButton ButtonTop1 = new(0x59, "Button Top 1");
        public static readonly XTouchMiniMcButton ButtonTop2 = new(0x5A, "Button Top 2");
        public static readonly XTouchMiniMcButton ButtonTop3 = new(0x28, "Button Top 3");
        public static readonly XTouchMiniMcButton ButtonTop4 = new(0x29, "Button Top 4");
        public static readonly XTouchMiniMcButton ButtonTop5 = new(0x2A, "Button Top 5");
        public static readonly XTouchMiniMcButton ButtonTop6 = new(0x2B, "Button Top 6");
        public static readonly XTouchMiniMcButton ButtonTop7 = new(0x2C, "Button Top 7");
        public static readonly XTouchMiniMcButton ButtonTop8 = new(0x2D, "Button Top 8");

        public static readonly XTouchMiniMcButton Encoder1 = new(0x20 + 0x00, "Button Encoder 1");
        public static readonly XTouchMiniMcButton Encoder2 = new(0x20 + 0x01, "Button Encoder 2");
        public static readonly XTouchMiniMcButton Encoder3 = new(0x20 + 0x02, "Button Encoder 3");
        public static readonly XTouchMiniMcButton Encoder4 = new(0x20 + 0x03, "Button Encoder 4");
        public static readonly XTouchMiniMcButton Encoder5 = new(0x20 + 0x04, "Button Encoder 5");
        public static readonly XTouchMiniMcButton Encoder6 = new(0x20 + 0x05, "Button Encoder 6");
        public static readonly XTouchMiniMcButton Encoder7 = new(0x20 + 0x06, "Button Encoder 7");
        public static readonly XTouchMiniMcButton Encoder8 = new(0x20 + 0x07, "Button Encoder 8");

        private XTouchMiniMcButton(byte value, string name) : base(value, name)
        {
        }
    }
}