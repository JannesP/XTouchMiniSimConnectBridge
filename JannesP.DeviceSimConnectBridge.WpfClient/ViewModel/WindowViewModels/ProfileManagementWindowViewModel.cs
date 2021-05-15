namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel.WindowViewModels
{
    public interface IProfileManagementWindowViewModel
    {
        IProfileManagementViewModel ProfileManagement { get; }
    }

    public class DesignTimeProfileManagementWindowViewModel : ViewModelBase, IProfileManagementWindowViewModel
    {
        public DesignTimeProfileManagementWindowViewModel()
        {
        }

        public IProfileManagementViewModel ProfileManagement { get; } = new DesignTimeProfileManagementViewModel();
    }

    public class ProfileManagementWindowViewModel : ViewModelBase, IProfileManagementWindowViewModel
    {
        public ProfileManagementWindowViewModel(ProfileManagementViewModel profileManagementViewModel)
        {
            ProfileManagement = profileManagementViewModel;
        }

        public IProfileManagementViewModel ProfileManagement { get; }
    }
}