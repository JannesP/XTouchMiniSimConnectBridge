namespace JannesP.DeviceSimConnectBridge.Device.XTouchMini
{
    internal class DeviceFader : IDeviceFader
    {
        public DeviceFader(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}