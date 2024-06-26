﻿using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.WpfApp.Extensions;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel;
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels;

namespace JannesP.DeviceSimConnectBridge.WpfApp.View.Windows
{
    /// <summary>
    /// Interaction logic for ProfileManagementWindow.xaml
    /// </summary>
    public partial class ProfileManagementWindow : Window
    {
        private readonly IProfileManagementWindowViewModel _viewModel;

        public ProfileManagementWindow(ProfileManagementWindowViewModel viewModel)
        {
            _viewModel = viewModel;
            base.DataContext = viewModel;
            InitializeComponent();
        }

        private void AddProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new TextInputDialog("Enter a name.", "Please enter a name for the new profile.", _viewModel.ProfileManagement.ValidateNewProfileName);
            if (dialog.ShowDialogCentered(this) == true && !string.IsNullOrWhiteSpace(dialog.Result))
            {
                _viewModel.ProfileManagement.CommandAddProfile.Execute(dialog.Result);
            }
        }

        private void DeleteProfileButton_Click(object sender, RoutedEventArgs e)
        {
            var b = (Button)sender;
            var profile = (IBindingProfileViewModel)b.DataContext;
            MessageBoxResult res = MessageBox.Show($"Are you really sure you want to delete the profile \"{profile.Name}\"? This cannot be undone!", "Delete profile?", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (res == MessageBoxResult.No) e.Handled = true;
        }

        private void ProfileListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not IBindingProfileViewModel profile)
            {
                throw new Exception("Expected to find a DataContext of type IBindingProfileViewModel.");
            }
            _viewModel.ProfileManagement.ChangeProfile(profile);
        }

        private void RenameProfileButton_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as FrameworkElement)?.DataContext is not IBindingProfileViewModel profile)
            {
                throw new Exception("Expected to find a DataContext of type IBindingProfileViewModel.");
            }

            var dialog = new TextInputDialog("Enter a name.", $"Please enter a new name for the profile \"{profile.Name}\".", _viewModel.ProfileManagement.ValidateNewProfileName);
            if (dialog.ShowDialogCentered(this) == true && !string.IsNullOrWhiteSpace(dialog.Result))
            {
                _viewModel.ProfileManagement.RenameProfile(profile, dialog.Result);
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
    }
}