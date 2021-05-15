using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTime;
using Microsoft.Extensions.DependencyInjection;
using static JannesP.DeviceSimConnectBridge.WpfApp.Managers.ProfileManager;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels
{
    public interface IMainWindowViewModel
    {
        IProfileManagementViewModel ProfileManagement { get; }
        ISimConnectManagerViewModel SimConnectManager { get; }
        IBindingProfileEditorViewModel ProfileEditor { get; } 
        ICommand CommandExit { get; }
        ICommand CommandOpenGithub { get; }
    }

    public class DesignTimeMainWindowViewModel : DesignTimeViewModel, IMainWindowViewModel
    {
        public IProfileManagementViewModel ProfileManagement { get; } = new DesignTimeProfileManagementViewModel();
        public ISimConnectManagerViewModel SimConnectManager { get; } = new DesignTimeSimConnectManagerViewModel();
        public ICommand CommandExit => EmptyCommand;
        public ICommand CommandOpenGithub => EmptyCommand;
        public IBindingProfileEditorViewModel ProfileEditor { get; } = new DesignTimeBindingProfileEditorViewModel();
    }

    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private readonly ProfileManager _profileManager;
        private readonly IServiceProvider _serviceProvider;

        private IBindingProfileEditorViewModel _profileEditor;
        

        public MainWindowViewModel(IServiceProvider serviceProvider, ProfileManagementViewModel profileManagementViewModel, SimConnectManagerViewModel simConnectManagerViewModel)
        {
            _serviceProvider = serviceProvider;
            ProfileManagement = profileManagementViewModel;
            SimConnectManager = simConnectManagerViewModel;
            _profileManager = serviceProvider.GetRequiredService<ProfileManager>();

            WeakEventManager<ProfileManager, ProfileChangedEventArgs>.AddHandler(_profileManager, nameof(ProfileManager.CurrentProfileChanged), ProfileManager_CurrentProfileChanged);

            _profileEditor = new BindingProfileEditorViewModel(_serviceProvider, _profileManager.GetCurrentProfile());
        }

        private void ProfileManager_CurrentProfileChanged(object? sender, ProfileChangedEventArgs eventArgs) 
            => ProfileEditor = new BindingProfileEditorViewModel(_serviceProvider, _profileManager.GetCurrentProfile());

        public IProfileManagementViewModel ProfileManagement { get; }
        public ISimConnectManagerViewModel SimConnectManager { get; }

        public ICommand CommandExit { get; } = new RelayCommand(async o => await App.GracefulShutdownAsync());

        public ICommand CommandOpenGithub { get; } = new RelayCommand(o =>
        {
            new Process() { StartInfo = new ProcessStartInfo(Constants.GithubLink) { UseShellExecute = true } }.Start();
        });

        public IBindingProfileEditorViewModel ProfileEditor
        {
            get => _profileEditor;
            private set
            {
                if (value != _profileEditor)
                {
                    _profileEditor = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
