using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTime
{
    public abstract class DesignTimeViewModel
    {
        protected static ICommand EmptyCommand { get; } = new RelayCommand(o => { });
    }
}