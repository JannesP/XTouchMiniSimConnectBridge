using System.Windows;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Extensions
{
    public static class WindowExtensions
    {
        public static bool? ShowDialogCentered(this Window window, Window owner)
        {
            window.Owner = owner;
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return window.ShowDialog();
        }
    }
}