using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels
{
    public interface IConfigureBindableActionDialogViewModel
    {
        string WindowTitle { get; }
        BindableActionViewModel BindableAction { get; }
    }

    public class DesignTimeConfigureBindableActionDialogViewModel : ViewModelBase, IConfigureBindableActionDialogViewModel
    {
        public DesignTimeConfigureBindableActionDialogViewModel()
        {
            BindableAction = new DesignTimeBindableActionViewModel();
        }

        public string WindowTitle => "DesignTime Title";
        public BindableActionViewModel BindableAction { get; }
    }

    public class ConfigureBindableActionDialogViewModel : ViewModelBase, IConfigureBindableActionDialogViewModel
    {
        public ConfigureBindableActionDialogViewModel(BindableActionViewModel action)
        {
            BindableAction = action;
        }

        public string WindowTitle => $"Configure: {BindableAction.Name}";
        public BindableActionViewModel BindableAction { get; }
    }
}
