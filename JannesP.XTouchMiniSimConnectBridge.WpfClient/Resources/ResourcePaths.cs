using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.XTouchMiniSimConnectBridge.WpfApp.Resources
{
    internal static class ResourcePaths
    {
        private static readonly string _baseResourcePath = "JannesP.XTouchMiniSimConnectBridge.WpfApp.Resources.";
        private static string Concat(string fileName) => _baseResourcePath + fileName;

        public static string TrayIcon = Concat("tray.ico");
    }
}
