namespace JannesP.DeviceSimConnectBridge.Device.XTouchMini
{
    internal class DeviceButton : IDeviceButton, IDeviceLed
    {
        public DeviceButton(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public int Id { get; }

        public string Name { get; }

        public DeviceLedState ValidStates { get; } = DeviceLedState.Off | DeviceLedState.On | DeviceLedState.Blinking;
    }
}