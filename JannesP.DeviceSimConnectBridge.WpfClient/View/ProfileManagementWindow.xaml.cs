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
using JannesP.DeviceSimConnectBridge.WpfApp.Extensions;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.View
{
    /// <summary>
    /// Interaction logic for ProfileManagementWindow.xaml
    /// </summary>
    public partial class ProfileManagementWindow : Window
    {
        private readonly ProfileManagementWindowViewModel _viewModel;

        public ProfileManagementWindow(ProfileManagementWindowViewModel viewModel)
        {
            _viewModel = viewModel;
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
            var dialog = new TextInputDialog("Enter a name.", "Please enter a name for the new profile.", _viewModel.ValidateNewProfileName);
            if (dialog.ShowDialogCentered() == true)
            {
                _viewModel.CommandAddProfile.Execute(dialog.Result);
            }
        }

        private void RenameProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not IBindingProfileViewModel profile)
            {
                throw new Exception("Expected to find a DataContext of type IBindingProfileViewModel.");
            }

            var dialog = new TextInputDialog("Enter a name.", $"Please enter a new name for the profile \"{profile.Name}\".", _viewModel.ValidateNewProfileName);
            if (dialog.ShowDialogCentered() == true && dialog.Result != null)
            {
                _viewModel.RenameProfile(profile, dialog.Result);
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    e.Handled = true;
                    base.Close();
                    break;
            }
        }

        private void ProfileListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not IBindingProfileViewModel profile)
            {
                throw new Exception("Expected to find a DataContext of type IBindingProfileViewModel.");
            }
            _viewModel.ChangeProfile(profile);
        }
    }
}
