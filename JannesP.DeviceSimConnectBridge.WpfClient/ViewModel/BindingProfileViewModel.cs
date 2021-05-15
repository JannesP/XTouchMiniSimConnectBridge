using System;
using System.Windows;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ViewModel
{
    public interface IBindingProfileViewModel
    {
        bool IsCurrent { get; }
        string? Name { get; set; }
        Guid? UniqueId { get; }
    }

    public class BindingProfileViewModel : ViewModelBase, IBindingProfileViewModel
    {
        private readonly BindingProfile _model;

        private bool _isCurrent = false;

        private string? _name;

        public BindingProfileViewModel(BindingProfile model, ProfileManager profileManager, ProfileRepository profileRepository)
        {
            _model = model;
            _name = model.Name;
            WeakEventManager<ProfileRepository, ProfileRepository.ProfileEventArgs>.AddHandler(profileRepository, nameof(profileRepository.ProfileChanged), ProfileRepository_ProfileChanged);
            WeakEventManager<ProfileManager, ProfileManager.ProfileChangedEventArgs>.AddHandler(profileManager, nameof(profileManager.CurrentProfileChanged), ProfileManager_OnCurrentProfileChanged);
        }

        public bool IsCurrent
        {
            get => _isCurrent;
            set
            {
                if (_isCurrent != value)
                {
                    _isCurrent = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public Guid? UniqueId => _model.UniqueId;

        private void ProfileManager_OnCurrentProfileChanged(object? sender, ProfileManager.ProfileChangedEventArgs e)
                                    => IsCurrent = e.NewProfile.UniqueId == UniqueId;

        private void ProfileRepository_ProfileChanged(object? sender, ProfileRepository.ProfileEventArgs e)
        {
            if (e.Profile.UniqueId == UniqueId)
            {
                Name = e.Profile.Name;
            }
        }
    }

    public class DesignTimeBindingProfileViewModel : IBindingProfileViewModel
    {
        private static int _mockNum = 0;

        public bool IsCurrent { get; set; } = false;
        public string? Name { get; set; } = $"DesignTime Profile {++_mockNum}";
        public Guid? UniqueId { get; } = Guid.NewGuid();
    }
}