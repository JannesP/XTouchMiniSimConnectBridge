namespace JannesP.XTouchMini
{
    public class XTouchMiniMcFader : ControlDefinition<XTouchMiniMcFader>
    {
        public static XTouchMiniMcFader Fader1 = new XTouchMiniMcFader(0x08, "Fader");

        private XTouchMiniMcFader(byte value, string name) : base(value, name)
        {
        }
    }
}