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
        public static bool? ShowDialogCentered(this Window window, Window parent)
        {
            double newLeft = parent.Left + (parent.ActualWidth / 2.0d) - (window.Width / 2.0d);
            double newTop = parent.Top + (parent.ActualHeight / 2.0d) - (window.Height / 2.0d);
            window.Left = newLeft;
            window.Top = newTop;
            return window.ShowDialog();
        }
    }
}
