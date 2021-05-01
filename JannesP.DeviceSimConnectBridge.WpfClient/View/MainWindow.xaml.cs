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
using JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.View
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(IServiceProvider serviceProvider, MainWindowViewModel viewModel)
        {
            base.DataContext = viewModel;
            InitializeComponent();
            _serviceProvider = serviceProvider;
        }

        private void ButtonOpenProfileManager_Click(object sender, RoutedEventArgs e)
        {
            ProfileManagementWindow profileManagementWindow = _serviceProvider.GetRequiredService<ProfileManagementWindow>();
            profileManagementWindow.ShowDialogCentered(this);
        }
    }
}
