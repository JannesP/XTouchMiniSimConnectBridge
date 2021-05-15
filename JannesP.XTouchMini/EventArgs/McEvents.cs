namespace JannesP.XTouchMini.EventArgs
{
    public abstract class XTouchMiniMcControlEventArgs<T> : System.EventArgs
        where T : ControlDefinition<T>
    {
        public XTouchMiniMcControlEventArgs(T control)
        {
            Control = control;
        }

        public T Control { get; }
    }

    public class XTouchMiniMcModeButtonEventArgs : XTouchMiniMcControlEventArgs<XTouchMiniMcButton>
    {
        public XTouchMiniMcModeButtonEventArgs(XTouchMiniMcButton button) : base(button)
        {
        }
    }

    public class XTouchMiniMcModeEncoderTurnedEventArgs : XTouchMiniMcControlEventArgs<XTouchMiniMcEncoder>
    {
        public XTouchMiniMcModeEncoderTurnedEventArgs(XTouchMiniMcEncoder encoder, int ticks) : base(encoder)
        {
            Ticks = ticks;
        }

        public int Ticks { get; }
    }

    public class XTouchMiniMcModeFaderMovedEventArgs : XTouchMiniMcControlEventArgs<XTouchMiniMcFader>
    {
        public XTouchMiniMcModeFaderMovedEventArgs(XTouchMiniMcFader fader, double value) : base(fader)
        {
            Value = value;
        }

        /// <summary>
        /// ranges from 0-1
        /// </summary>
        public double Value { get; }
    }
}