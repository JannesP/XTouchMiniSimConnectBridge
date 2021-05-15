using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels
{
    public interface IConfigureBindableActionDialogViewModel
    {
        BindableActionViewModel BindableAction { get; }
        string WindowTitle { get; }
    }

    public class ConfigureBindableActionDialogViewModel : ViewModelBase, IConfigureBindableActionDialogViewModel
    {
        public ConfigureBindableActionDialogViewModel(BindableActionViewModel action)
        {
            BindableAction = action;
        }

        public BindableActionViewModel BindableAction { get; }
        public string WindowTitle => $"Configure: {BindableAction.Name}";
    }

    public class DesignTimeConfigureBindableActionDialogViewModel : ViewModelBase, IConfigureBindableActionDialogViewModel
    {
        public DesignTimeConfigureBindableActionDialogViewModel()
        {
            BindableAction = new DesignTimeBindableActionViewModel();
        }

        public BindableActionViewModel BindableAction { get; }
        public string WindowTitle => "DesignTime Title";
    }
}