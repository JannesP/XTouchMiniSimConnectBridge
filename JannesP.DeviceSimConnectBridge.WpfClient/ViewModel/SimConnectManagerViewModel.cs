using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel
{
    public interface ISimConnectManagerViewModel
    {
        Brush ConnectionStateBrush { get; }
        string ConnectionStateText { get; }
    }

    public class DesignTimeSimConnectManagerViewModel : ISimConnectManagerViewModel
    {
        public Brush ConnectionStateBrush => Brushes.Magenta;
        public string ConnectionStateText => "Designing";
    }

    public class SimConnectManagerViewModel : ViewModelBase, ISimConnectManagerViewModel
    {
        private readonly SimConnectManager? _simConnectManager;
        private string _connectionStateText;
        private Brush _connectionStateBrush;

        public SimConnectManagerViewModel(SimConnectManager simConnectManager)
        {
            _simConnectManager = simConnectManager;
            WeakEventManager<SimConnectManager, SimConnectManager.StateChangedEventArgs>
                .AddHandler(simConnectManager, nameof(SimConnectManager.StateChanged), OnSimConnectManager_StateChanged);
            _connectionStateText = GetTextForState(_simConnectManager.ConnectionState);
            _connectionStateBrush = GetBrushForState(_simConnectManager.ConnectionState);
        }

        public string ConnectionStateText
        {
            get => _connectionStateText;
            private set
            {
                if (_connectionStateText != value)
                {
                    _connectionStateText = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush ConnectionStateBrush
        {
            get => _connectionStateBrush;
            private set
            {
                if (_connectionStateBrush != value)
                {
                    _connectionStateBrush = value;
                    OnPropertyChanged();
                }
            }
        }

        private void OnSimConnectManager_StateChanged(object? sender, SimConnectManager.StateChangedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                ConnectionStateText = GetTextForState(e.NewState);
                ConnectionStateBrush = GetBrushForState(e.NewState);
            });
        }

        private static string GetTextForState(SimConnectManager.State state) => state.ToString();
        private static Brush GetBrushForState(SimConnectManager.State state) => state switch
        {
            SimConnectManager.State.Connected => Brushes.Green,
            SimConnectManager.State.Connecting => Brushes.Yellow,
            SimConnectManager.State.ConnectingWaitingForResponse => Brushes.YellowGreen,
            SimConnectManager.State.Disconnected => Brushes.Red,
            SimConnectManager.State.Disconnecting => Brushes.Orange,
            _ => Brushes.Purple,
        };

        
    }
}
