using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels
{
    public interface IMainWindowViewModel
    {
        ISimConnectManagerViewModel SimConnectManagerViewModel { get; }
    }

    public class DesignTimeMainWindowViewModel : IMainWindowViewModel
    {
        public ISimConnectManagerViewModel SimConnectManagerViewModel { get; } = new DesignTimeSimConnectManagerViewModel();
    }

    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        public MainWindowViewModel(SimConnectManagerViewModel simConnectManagerViewModel)
        {
            SimConnectManagerViewModel = simConnectManagerViewModel;
        }

        public ISimConnectManagerViewModel SimConnectManagerViewModel { get; }
    }
}
