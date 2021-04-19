using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.XTouchMiniSimConnectBridge.WpfApp.Resources
{
    internal static class ResourcePaths
    {
        private static readonly string BaseResourcePath = "JannesP.XTouchMiniSimConnectBridge.WpfApp.Resources.";
        private static string Concat(string fileName) => BaseResourcePath + fileName;

        public static string TrayIcon = Concat("tray.ico");
    }
}
