using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using JannesP.DeviceSimConnectBridge.WpfApp.Manager;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel
{
    public class SimConnectManagerViewModel : ViewModelBase
    {
        private readonly SimConnectManager? _simConnectManager;
        private string _connectionStateText;

        public SimConnectManagerViewModel(SimConnectManager simConnectManager)
        {
            _simConnectManager = simConnectManager;
            _simConnectManager.StateChanged += OnSimConnectManager_StateChanged;
            _connectionStateText = _simConnectManager.ConnectionState.ToString();
        }

        private void OnSimConnectManager_StateChanged(object? sender, SimConnectManager.StateChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ConnectionStateText = e.NewState.ToString();
            });
        }

        public SimConnectManagerViewModel()
        {
            _connectionStateText = "designing ...";
        }

        public string ConnectionStateText
        {
            get => _connectionStateText; private set
            {
                if (_connectionStateText != value)
                {
                    _connectionStateText = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
