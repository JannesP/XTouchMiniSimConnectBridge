using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Shapes;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.View
{
    /// <summary>
    /// Interaction logic for ProfileManagementWindow.xaml
    /// </summary>
    public partial class ProfileManagementWindow : Window
    {
        public ProfileManagementWindow(ProfileManagementWindowViewModel viewModel)
        {
            base.DataContext = viewModel;
            InitializeComponent();
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            var profile = (IBindingProfileViewModel)b.DataContext;
            MessageBoxResult res = MessageBox.Show($"Are you really sure you want to delete the profile \"{profile.Name}\"? This cannot be undone!", "Delete profile?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.No) e.Handled = true;
        }

        private void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
