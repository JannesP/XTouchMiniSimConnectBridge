using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Manager
{
    internal class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon _trayIcon;

        public TrayIconManager(string iconResourceName)
        {
            _trayIcon = CreateIcon(iconResourceName);
        }

        private NotifyIcon CreateIcon(string iconResourceName)
        {
            var newIcon = new NotifyIcon();
            newIcon.DoubleClick += OnDoubleClick;
            var menu = new ContextMenuStrip();
            menu.Items.Add("Exit", null, OnItemExitClick);
            newIcon.ContextMenuStrip = menu;
            newIcon.Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream(iconResourceName) ?? throw new InvalidOperationException("Error reading resource stream."));
            return newIcon;
        }

        public bool IconVisible
        {
            get
            {
                return _trayIcon?.Visible ?? false;
            }
            set
            {
                if (_trayIcon == null)
                {
                    throw new InvalidOperationException("You can only change the visibility after the icon was created with CreateIcon!");
                }
                _trayIcon.Visible = value;
            }
        }

        public event EventHandler? ItemExitClick;
        public event EventHandler? DoubleClick;

        private void OnItemExitClick(object? icon, EventArgs e) => ItemExitClick?.Invoke(this, EventArgs.Empty);

        private void OnDoubleClick(object? icon, EventArgs e) => DoubleClick?.Invoke(this, EventArgs.Empty);

        public void Dispose() => _trayIcon?.Dispose();
    }
}
