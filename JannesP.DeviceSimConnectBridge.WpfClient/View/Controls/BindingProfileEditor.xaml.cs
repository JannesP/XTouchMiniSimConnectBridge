using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
                var dialog = new ConfigureBindableActionDialog(action)
                {
                    WindowStartupLocation = WindowStartupLocation.CenterOwner,
                };
                dialog.ShowDialog();
            }
        }
    }
}
