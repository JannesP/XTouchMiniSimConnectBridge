namespace JannesP.DeviceSimConnectBridge.WpfApp.Resources
{
    public static class ResourcePaths
    {
        private static readonly string _baseResourcePath = "JannesP.DeviceSimConnectBridge.WpfApp.Resources.";
        public static string TrayIcon => Concat("tray.ico");

        private static string Concat(string fileName) => _baseResourcePath + fileName;
    }
}