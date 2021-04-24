namespace JannesP.DeviceSimConnectBridge.Device
{
    public interface IDeviceButton : IDeviceInput { }

    public interface IDeviceButtonEventArgs
    {
        IDeviceButton Button { get; }
    }
}
