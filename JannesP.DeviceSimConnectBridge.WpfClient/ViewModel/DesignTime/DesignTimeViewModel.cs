using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTime
{
    public abstract class DesignTimeViewModel
    {
        protected static ICommand EmptyCommand { get; } = new RelayCommand(o => { });
    }
}
