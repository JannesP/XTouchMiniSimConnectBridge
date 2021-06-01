using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindingProfileEditorViewModels;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.DesignTime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using static JannesP.DeviceSimConnectBridge.WpfApp.Managers.ProfileManager;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels
{
    public interface IMainWindowViewModel
    {
        ICommand CommandApplyProfileChanges { get; }
        ICommand CommandExit { get; }
        ICommand CommandOpenGithub { get; }
        ICommand CommandRevertProfileChanges { get; }
        IBindingProfileEditorViewModel ProfileEditor { get; }
        IProfileManagementViewModel ProfileManagement { get; }
        ISimConnectManagerViewModel SimConnectManager { get; }
    }

    public class DesignTimeMainWindowViewModel : DesignTimeViewModel, IMainWindowViewModel
    {
        public ICommand CommandApplyProfileChanges => EmptyCommand;
        public ICommand CommandExit => EmptyCommand;
        public ICommand CommandOpenGithub => EmptyCommand;
        public ICommand CommandRevertProfileChanges => EmptyCommand;
        public IBindingProfileEditorViewModel ProfileEditor { get; } = new DesignTimeBindingProfileEditorViewModel();
        public IProfileManagementViewModel ProfileManagement { get; } = new DesignTimeProfileManagementViewModel();
        public ISimConnectManagerViewModel SimConnectManager { get; } = new DesignTimeSimConnectManagerViewModel();
    }

    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        private readonly RelayCommand _commandApplyProfileChanges;
        private readonly RelayCommand _commandRevertProfileChanges;
        private readonly DeviceBindingManager _deviceBindingManager;
        private readonly ILogger<MainWindowViewModel> _logger;
        private readonly ProfileManager _profileManager;
        private readonly IServiceProvider _serviceProvider;
        private IBindingProfileEditorViewModel _profileEditor;

        public MainWindowViewModel(IServiceProvider serviceProvider, ProfileManagementViewModel profileManagementViewModel, SimConnectManagerViewModel simConnectManagerViewModel)
        {
            _serviceProvider = serviceProvider;
            ProfileManagement = profileManagementViewModel;
            SimConnectManager = simConnectManagerViewModel;
            _profileManager = serviceProvider.GetRequiredService<ProfileManager>();
            _deviceBindingManager = serviceProvider.GetRequiredService<DeviceBindingManager>();
            _logger = serviceProvider.GetRequiredService<ILogger<MainWindowViewModel>>();

            WeakEventManager<ProfileManager, ProfileChangedEventArgs>.AddHandler(_profileManager, nameof(ProfileManager.CurrentProfileChanged), ProfileManager_CurrentProfileChanged);

            _profileEditor = new BindingProfileEditorViewModel(_serviceProvider, _profileManager.GetCurrentProfile());
            _profileEditor.CommandApplyChanges.CanExecuteChanged += ProfileEditorCommandApplyChanges_CanExecuteChanged;
            _profileEditor.CommandRevertChanges.CanExecuteChanged += ProfileEditorCommandRevertChanges_CanExecuteChanged;

            _commandApplyProfileChanges = new NotifiedRelayCommand(o =>
            {
                ProfileEditor.CommandApplyChanges.Execute(o);
                _ = _deviceBindingManager.EnableAsync(_profileManager.GetCurrentProfile()).ContinueWith(t =>
                {
                    _logger.LogError(t.Exception, "Error enabling current profile.");
                }, TaskContinuationOptions.OnlyOnFaulted);
            }, o => ProfileEditor.CommandApplyChanges.CanExecute(o), this, nameof(ProfileEditor));

            _commandRevertProfileChanges = new NotifiedRelayCommand(ProfileEditor.CommandApplyChanges.Execute, ProfileEditor.CommandRevertChanges.CanExecute, this, nameof(ProfileEditor));
        }

        public ICommand CommandApplyProfileChanges => _commandApplyProfileChanges;
        public ICommand CommandExit { get; } = new RelayCommand(async o => await App.GracefulShutdownAsync());

        public ICommand CommandOpenGithub { get; } = new RelayCommand(o =>
        {
            new Process() { StartInfo = new ProcessStartInfo(Constants.GithubLink) { UseShellExecute = true } }.Start();
        });

        public ICommand CommandRevertProfileChanges => _commandRevertProfileChanges;

        public IBindingProfileEditorViewModel ProfileEditor
        {
            get => _profileEditor;
            private set
            {
                if (value != _profileEditor)
                {
                    if (_profileEditor != null)
                    {
                        _profileEditor.CommandApplyChanges.CanExecuteChanged -= ProfileEditorCommandApplyChanges_CanExecuteChanged;
                        _profileEditor.CommandRevertChanges.CanExecuteChanged -= ProfileEditorCommandRevertChanges_CanExecuteChanged;
                    }
                    _profileEditor = value;
                    if (_profileEditor != null)
                    {
                        _profileEditor.CommandApplyChanges.CanExecuteChanged += ProfileEditorCommandApplyChanges_CanExecuteChanged;
                        _profileEditor.CommandRevertChanges.CanExecuteChanged += ProfileEditorCommandRevertChanges_CanExecuteChanged;
                    }
                    OnPropertyChanged();
                }
            }
        }

        public IProfileManagementViewModel ProfileManagement { get; }

        public ISimConnectManagerViewModel SimConnectManager { get; }

        private void ProfileEditorCommandApplyChanges_CanExecuteChanged(object? sender, EventArgs e)
        {
            _commandApplyProfileChanges.FireCanExecuteChanged();
        }

        private void ProfileEditorCommandRevertChanges_CanExecuteChanged(object? sender, EventArgs e)
        {
            _commandRevertProfileChanges.FireCanExecuteChanged();
        }

        private void ProfileManager_CurrentProfileChanged(object? sender, ProfileChangedEventArgs eventArgs)
                                                    => ProfileEditor = new BindingProfileEditorViewModel(_serviceProvider, _profileManager.GetCurrentProfile());
    }
}