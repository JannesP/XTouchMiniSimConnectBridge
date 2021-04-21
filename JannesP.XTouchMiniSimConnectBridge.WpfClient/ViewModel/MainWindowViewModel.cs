using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.XTouchMiniSimConnectBridge.WpfApp.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public MainWindowViewModel(SimConnectManagerViewModel simConnectManagerViewModel)
        {
            SimConnectManagerViewModel = simConnectManagerViewModel;
        }

        public MainWindowViewModel()
        {
            SimConnectManagerViewModel = new SimConnectManagerViewModel();
        }

        public SimConnectManagerViewModel SimConnectManagerViewModel { get; }
    }
}
