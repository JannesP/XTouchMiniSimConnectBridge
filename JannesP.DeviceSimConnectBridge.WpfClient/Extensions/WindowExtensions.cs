using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Extensions
{
    public static class WindowExtensions
    {
        public static bool? ShowDialogCentered(this Window window)
        {
            window.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            return window.ShowDialog();
        }
    }
}
