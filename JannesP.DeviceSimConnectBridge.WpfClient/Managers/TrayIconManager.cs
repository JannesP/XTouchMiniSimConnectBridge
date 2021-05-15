using System;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Managers
{
    internal class TrayIconManager : IDisposable
    {
        private readonly NotifyIcon _trayIcon;

        public TrayIconManager(string iconResourceName)
        {
            _trayIcon = CreateIcon(iconResourceName);
        }

        public event EventHandler? DoubleClick;

        public event EventHandler? ItemExitClick;

        public bool IconVisible
        {
            get => _trayIcon?.Visible ?? false;
            set
            {
                if (_trayIcon == null)
                {
                    throw new InvalidOperationException("You can only change the visibility after the icon was created with CreateIcon!");
                }
                _trayIcon.Visible = value;
            }
        }

        public void Dispose() => _trayIcon?.Dispose();

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

        private void OnDoubleClick(object? icon, EventArgs e) => DoubleClick?.Invoke(this, EventArgs.Empty);

        private void OnItemExitClick(object? icon, EventArgs e) => ItemExitClick?.Invoke(this, EventArgs.Empty);
    }
}