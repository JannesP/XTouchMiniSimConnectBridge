using System;
using System.Collections.Generic;
using System.Text;

namespace JannesP.XTouchMini
{
    public class XTouchMiniMcButton : ControlDefinition<XTouchMiniMcButton>
    {
        public static XTouchMiniMcButton ButtonTop1 = new XTouchMiniMcButton(0x59, "Button Top 1");
        public static XTouchMiniMcButton ButtonTop2 = new XTouchMiniMcButton(0x5A, "Button Top 2");
        public static XTouchMiniMcButton ButtonTop3 = new XTouchMiniMcButton(0x28, "Button Top 3");
        public static XTouchMiniMcButton ButtonTop4 = new XTouchMiniMcButton(0x29, "Button Top 4");
        public static XTouchMiniMcButton ButtonTop5 = new XTouchMiniMcButton(0x2A, "Button Top 5");
        public static XTouchMiniMcButton ButtonTop6 = new XTouchMiniMcButton(0x2B, "Button Top 6");
        public static XTouchMiniMcButton ButtonTop7 = new XTouchMiniMcButton(0x2C, "Button Top 7");
        public static XTouchMiniMcButton ButtonTop8 = new XTouchMiniMcButton(0x2D, "Button Top 8");

        public static XTouchMiniMcButton ButtonBottom1 = new XTouchMiniMcButton(0x57, "Button Bottom 1");
        public static XTouchMiniMcButton ButtonBottom2 = new XTouchMiniMcButton(0x58, "Button Bottom 2");
        public static XTouchMiniMcButton ButtonBottom3 = new XTouchMiniMcButton(0x5B, "Button Bottom 3");
        public static XTouchMiniMcButton ButtonBottom4 = new XTouchMiniMcButton(0x5C, "Button Bottom 4");
        public static XTouchMiniMcButton ButtonBottom5 = new XTouchMiniMcButton(0x56, "Button Bottom 5");
        public static XTouchMiniMcButton ButtonBottom6 = new XTouchMiniMcButton(0x5D, "Button Bottom 6");
        public static XTouchMiniMcButton ButtonBottom7 = new XTouchMiniMcButton(0x5E, "Button Bottom 7");
        public static XTouchMiniMcButton ButtonBottom8 = new XTouchMiniMcButton(0x5F, "Button Bottom 8");

        public static XTouchMiniMcButton ButtonLayerA = new XTouchMiniMcButton(0x54, "Button Layer A");
        public static XTouchMiniMcButton ButtonLayerB = new XTouchMiniMcButton(0x55, "Button Layer B");

        public static XTouchMiniMcButton Encoder1 = new XTouchMiniMcButton(0x20 + 0x00, "Button Encoder 1");
        public static XTouchMiniMcButton Encoder2 = new XTouchMiniMcButton(0x20 + 0x01, "Button Encoder 2");
        public static XTouchMiniMcButton Encoder3 = new XTouchMiniMcButton(0x20 + 0x02, "Button Encoder 3");
        public static XTouchMiniMcButton Encoder4 = new XTouchMiniMcButton(0x20 + 0x03, "Button Encoder 4");
        public static XTouchMiniMcButton Encoder5 = new XTouchMiniMcButton(0x20 + 0x04, "Button Encoder 5");
        public static XTouchMiniMcButton Encoder6 = new XTouchMiniMcButton(0x20 + 0x05, "Button Encoder 6");
        public static XTouchMiniMcButton Encoder7 = new XTouchMiniMcButton(0x20 + 0x06, "Button Encoder 7");
        public static XTouchMiniMcButton Encoder8 = new XTouchMiniMcButton(0x20 + 0x07, "Button Encoder 8");

        private XTouchMiniMcButton(byte value, string name) : base(value, name) { }
    }
}
