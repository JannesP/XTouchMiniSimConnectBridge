namespace JannesP.DeviceSimConnectBridge.Device
{
    public interface IDeviceFader : IDeviceInput { }

    public interface IDeviceFaderEventArgs
    {
        IDeviceFader Fader { get; }
        /// <summary>
        /// Value from 0 to 1
        /// </summary>
        double Value { get; }

    }
}
