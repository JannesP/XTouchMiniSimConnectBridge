using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using JannesP.DeviceSimConnectBridge.WpfApp.Exceptions;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.Utility.Wpf;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels
{
    public interface IProfileManagementWindowViewModel
    {
        IProfileManagementViewModel ProfileManagement { get; }
    }

    public class DesignTimeProfileManagementWindowViewModel : ViewModelBase, IProfileManagementWindowViewModel
    {
        public IProfileManagementViewModel ProfileManagement { get; } = new DesignTimeProfileManagementViewModel();

        public DesignTimeProfileManagementWindowViewModel() { }
    }

    public class ProfileManagementWindowViewModel : ViewModelBase, IProfileManagementWindowViewModel
    {
        public IProfileManagementViewModel ProfileManagement { get; }

        public ProfileManagementWindowViewModel(ProfileManagementViewModel profileManagementViewModel)
        {
            ProfileManagement = profileManagementViewModel;
        }
    }
}
