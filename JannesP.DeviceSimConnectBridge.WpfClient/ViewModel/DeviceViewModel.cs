using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTimeModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel
{
    public class DeviceViewModel : ViewModelBase
    {
        private readonly IDevice _device;

        public DeviceViewModel() : this(new DesignTimeDevice()) { }

        public DeviceViewModel(IDevice device)
        {
            _device = device;
            WeakEventManager<IDevice, EventArgs>.AddHandler(device, nameof(IDevice.Connected), Device_Connected);
            WeakEventManager<IDevice, EventArgs>.AddHandler(device, nameof(IDevice.Disconnected), Device_Disconnected);
        }

        private void Device_Connected(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(StatusString));
            });
        }

        private void Device_Disconnected(object? sender, EventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                OnPropertyChanged(nameof(StatusString));
            });
        }

        public string DisplayName => _device.DeviceName;
        public string StatusString => _device.IsConnected ? "Connected" : "Disconnected";
    }
}
