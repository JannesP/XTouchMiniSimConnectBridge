using System.Windows;
using System.Windows.Controls;
using JannesP.DeviceSimConnectBridge.WpfApp.Extensions;
using JannesP.DeviceSimConnectBridge.WpfApp.View.Windows;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.BindableActionSettingsViewModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.View.Controls
{
    /// <summary>
    /// Interaction logic for BindingProfileEditor.xaml
    /// </summary>
    public partial class BindingProfileEditor : UserControl
    {
        public BindingProfileEditor()
        {
            InitializeComponent();
        }

        private void ButtonConfigureBindableAction_Click(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement s && s.DataContext is BindableActionViewModel action)
            {
                new ConfigureBindableActionDialog(action).ShowDialogCentered(Window.GetWindow(this));
            }
        }
    }
}