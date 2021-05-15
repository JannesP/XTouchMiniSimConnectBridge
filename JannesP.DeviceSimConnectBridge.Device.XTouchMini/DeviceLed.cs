namespace JannesP.DeviceSimConnectBridge.Device.XTouchMini
{
    public class DeviceLed : IDeviceLed
    {
        public DeviceLed(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }
        public string Name { get; }
        public DeviceLedState ValidStates { get; } = DeviceLedState.Off | DeviceLedState.On | DeviceLedState.Blinking;
    }
}