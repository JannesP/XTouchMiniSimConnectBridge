using System.Collections.Generic;

namespace JannesP.XTouchMini
{
    public abstract class ControlDefinition<T> where T : ControlDefinition<T>
    {
        static ControlDefinition()
        {
            //if this isn't here the collection won't be filled with entries if none of the subclass members are accessed
            System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(typeof(T).TypeHandle);
        }

        protected ControlDefinition(byte midiCode, string name)
        {
            MidiCode = midiCode;
            Name = name;
            Controls.Add(midiCode, (T)this);
        }

        public static Dictionary<byte, T> Controls { get; } = new Dictionary<byte, T>();
        public byte MidiCode { get; }
        public string Name { get; }
    }
}