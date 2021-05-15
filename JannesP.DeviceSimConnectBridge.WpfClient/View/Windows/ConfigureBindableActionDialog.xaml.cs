using System.Windows;
using System.Windows.Controls;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.View.Windows
{
    /// <summary>
    /// Interaction logic for ConfigureBindableActionDialog.xaml
    /// </summary>
    public partial class ConfigureBindableActionDialog : Window
    {
        public ConfigureBindableActionDialog(BindableActionViewModel action)
        {
            base.DataContext = new ConfigureBindableActionDialogViewModel(action);
            InitializeComponent();
        }
    }

    public class SettingTemplateSelector : DataTemplateSelector
    {
        public DataTemplate? IntTemplate { get; set; }
        public DataTemplate? StringTemplate { get; set; }

        public override DataTemplate? SelectTemplate(object item, DependencyObject container)
        {
            if (item is StringActionSettingViewModel) return StringTemplate;
            else if (item is IntActionSettingViewModel) return IntTemplate;
            else return base.SelectTemplate(item, container);
        }
    }
}