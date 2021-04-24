namespace JannesP.DeviceSimConnectBridge.Device
{
    public interface IDeviceEncoder : IDeviceInput { }

    public interface IDeviceEncoderEventArgs
    {
        IDeviceEncoder Encoder { get; }
        /// <summary>
        /// The amount of steps turned since the last event. Positive steps are clockwise, negative steps are anticlockwise.
        /// </summary>
        int Steps { get; }
    }
}
