using System;

namespace JannesP.DeviceSimConnectBridge.Device
{
    public interface IDeviceEncoder : IDeviceInput { }

    public class DeviceEncoderEventArgs : EventArgs
    {
        public DeviceEncoderEventArgs(IDeviceEncoder encoder, int steps)
        {
            Encoder = encoder;
            Steps = steps;
        }

        public IDeviceEncoder Encoder { get; }

        /// <summary>
        /// The amount of steps turned since the last event. Positive steps are clockwise, negative steps are anticlockwise.
        /// </summary>
        public int Steps { get; }
    }
}