namespace JannesP.DeviceSimConnectBridge.Device.XTouchMini
{
    internal class DeviceEncoder : IDeviceEncoder
    {
        public DeviceEncoder(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }
    }
}