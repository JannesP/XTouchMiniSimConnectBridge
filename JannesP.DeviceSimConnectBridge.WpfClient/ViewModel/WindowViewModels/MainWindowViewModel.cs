using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTime;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels
{
    public interface IMainWindowViewModel
    {
        IProfileManagementViewModel ProfileManagement { get; }
        ISimConnectManagerViewModel SimConnectManager { get; }
        ICommand CommandExit { get; }
        ICommand CommandOpenGithub { get; }
    }

    public class DesignTimeMainWindowViewModel : DesignTimeViewModel, IMainWindowViewModel
    {
        public IProfileManagementViewModel ProfileManagement { get; } = new DesignTimeProfileManagementViewModel();
        public ISimConnectManagerViewModel SimConnectManager { get; } = new DesignTimeSimConnectManagerViewModel();
        public ICommand CommandExit => EmptyCommand;
        public ICommand CommandOpenGithub => EmptyCommand;
    }

    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        public MainWindowViewModel(ProfileManagementViewModel profileManagementViewModel, SimConnectManagerViewModel simConnectManagerViewModel)
        {
            ProfileManagement = profileManagementViewModel;
            SimConnectManager = simConnectManagerViewModel;
        }

        public IProfileManagementViewModel ProfileManagement { get; }
        public ISimConnectManagerViewModel SimConnectManager { get; }

        public ICommand CommandExit { get; } = new RelayCommand(o =>
        {
            Application.Current.Shutdown(0);
        });

        public ICommand CommandOpenGithub { get; } = new RelayCommand(o => 
        {
            new Process() { StartInfo = new ProcessStartInfo(Constants.GithubLink) { UseShellExecute = true } }.Start();
        });
    }
}
