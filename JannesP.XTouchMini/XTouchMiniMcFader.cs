namespace JannesP.XTouchMini
{
    public class XTouchMiniMcFader : ControlDefinition<XTouchMiniMcFader>
    {
        public static readonly XTouchMiniMcFader Fader1 = new(0x08, "Fader");

        private XTouchMiniMcFader(byte value, string name) : base(value, name)
        {
        }
    }
}