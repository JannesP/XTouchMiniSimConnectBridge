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
using JannesP.DeviceSimConnectBridge.WpfApp.Extensions;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.View.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly MainWindowViewModel _viewModel;

        public MainWindow(IServiceProvider serviceProvider, MainWindowViewModel viewModel)
        {
            base.DataContext = viewModel;
            InitializeComponent();
            _serviceProvider = serviceProvider;
            _viewModel = viewModel;
        }

        private void ButtonOpenProfileManager_Click(object sender, RoutedEventArgs e)
        {
            ProfileManagementWindow profileManagementWindow = _serviceProvider.GetRequiredService<ProfileManagementWindow>();
            profileManagementWindow.ShowDialogCentered();
        }

        private void MenuItemOpenAbout_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Here is going to be an about page at some point :)");
        }

        private void MenuItemOpenSettings_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Here is going to be a settings page at some point :)");
        }

        private void ComboBoxProfile_SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                if (comboBox.SelectedItem is IBindingProfileViewModel profile)
                {
                    _viewModel?.ProfileManagement.ChangeProfile(profile);
                }
            }            
        }
    }
}
