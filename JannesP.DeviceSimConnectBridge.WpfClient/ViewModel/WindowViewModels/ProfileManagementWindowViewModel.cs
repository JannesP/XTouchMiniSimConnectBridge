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
        IBindingProfileViewModel? CurrentProfile { get; }
        bool IsLoaded { get; }
        bool IsLoading { get; }
        ObservableCollection<IBindingProfileViewModel> ProfileList { get; }
        ICommand CommandDeleteProfile { get; }
        ICommand CommandAddProfile { get; }
        string? ErrorMessage { get; }
        ValidationResult? ValidateNewProfileName(object value);
    }

    public class DesignTimeProfileManagementWindowViewModel : IProfileManagementWindowViewModel
    {
        public DesignTimeProfileManagementWindowViewModel()
        {
            ProfileList = new ObservableCollection<IBindingProfileViewModel>()
            {
                new DesignTimeBindingProfileViewModel(),
                new DesignTimeBindingProfileViewModel() { IsCurrent = true },
                new DesignTimeBindingProfileViewModel(),
                new DesignTimeBindingProfileViewModel(),
            };
            CurrentProfile = ProfileList[2];
        }
        public IBindingProfileViewModel? CurrentProfile { get; private set; }
        public bool IsLoaded => true;
        public bool IsLoading => false;
        public ObservableCollection<IBindingProfileViewModel> ProfileList { get; private set; }
        public ICommand CommandDeleteProfile { get; } = RelayCommand.Empty;
        public ICommand CommandAddProfile { get; } = RelayCommand.Empty;
        public string? ErrorMessage => "Design Error YEP";
        public ValidationResult? ValidateNewProfileName(object value) => null;
    }

    public class ProfileManagementWindowViewModel : ViewModelBase, IProfileManagementWindowViewModel
    {
        private readonly ProfileManager _profileManager;
        private readonly ProfileRepository _profileRepository;
        private readonly ILogger<ProfileManagementWindowViewModel> _logger;
        private readonly RelayCommand _commandDeleteProfile;
        private bool _isLoaded = false;
        private IBindingProfileViewModel? _currentProfile;
        private string? _errorMessage;

        public ProfileManagementWindowViewModel(ProfileManager profileManager, ProfileRepository profileRepository, ILogger<ProfileManagementWindowViewModel> logger)
        {
            _profileManager = profileManager;
            _profileRepository = profileRepository;
            _logger = logger;
            WeakEventManager<ProfileManager, ProfileManager.ProfileChangedEventArgs>.AddHandler(profileManager, nameof(profileManager.CurrentProfileChanged), ProfileManager_OnCurrentProfileChanged);
            WeakEventManager<ProfileRepository, ProfileRepository.ProfileEventArgs>.AddHandler(profileRepository, nameof(profileRepository.ProfileAdded), ProfileRepository_OnProfileAdded);
            WeakEventManager<ProfileRepository, ProfileRepository.ProfileEventArgs>.AddHandler(profileRepository, nameof(profileRepository.ProfileRemoved), ProfileRepository_OnProfileRemoved);
            ProfileList = new ObservableCollection<IBindingProfileViewModel>();
            _commandDeleteProfile = new RelayCommand(DeleteProfile, CanDeleteProfile);
            CommandAddProfile = new RelayCommand(AddNewProfile);
            LoadProfiles();
        }

        private void ProfileRepository_OnProfileRemoved(object? sender, ProfileRepository.ProfileEventArgs e)
        {
            IBindingProfileViewModel profile = ProfileList.Single(p => p.UniqueId == e.Profile.UniqueId);
            ProfileList.Remove(profile);
        }

        private void ProfileRepository_OnProfileAdded(object? sender, ProfileRepository.ProfileEventArgs e) 
            => ProfileList.Add(new BindingProfileViewModel(e.Profile, _profileManager, _profileRepository));

        private void AddNewProfile(object? obj)
        {
            ValidationResult? validation = ValidateNewProfileName(obj);
            if (validation != null)
            {
                ErrorMessage = validation.ErrorMessage ?? "An error occured.";
                return;
            }
            _profileManager.CreateNewProfile((string)obj!);
            ErrorMessage = null;
        }

        public ValidationResult? ValidateNewProfileName(object? value)
        {
            if (value is string newName)
            {
                string? error = _profileRepository.IsNewProfileNameValid(newName);
                return error == null ? null : new ValidationResult(error);
            }
            else
            {
                throw new ArgumentException("Parameter is expected to be of type string.", nameof(value));
            }
        }

        private void LoadProfiles()
        {
            try
            {
                Guid? currentProfileId = _profileManager.GetCurrentProfile().UniqueId;
                if (currentProfileId == null) throw new UserMessageException("Current profile is missing a UniqueId.", "There was an error loading the current profile.");

                List<BindingProfile>? profiles = _profileRepository.GetProfiles();
                foreach (BindingProfile profile in profiles)
                {
                    var newViewModel = new BindingProfileViewModel(profile, _profileManager, _profileRepository)
                    {
                        IsCurrent = profile.UniqueId == currentProfileId,
                    };
                    ProfileList.Add(newViewModel);
                }

                CurrentProfile = ProfileList.SingleOrDefault(p => p.UniqueId == currentProfileId);
                _commandDeleteProfile.FireCanExecuteChanged();
            }
            finally
            {
                IsLoaded = true;
            }
        }

        public void RenameProfile(IBindingProfileViewModel profile, string newName)
        {
            ValidationResult? validationResult = ValidateNewProfileName(newName);
            if (validationResult == null)
            {
                if (!profile.UniqueId.HasValue) throw new Exception("Profile didn't have a UniqueId");
                _profileRepository.RenameProfile(profile.UniqueId.Value, newName);
            }
            else
            {
                ErrorMessage = validationResult.ErrorMessage ?? "A validation error occured.";
            }
        }

        public void ChangeProfile(IBindingProfileViewModel profile)
        {
            if (!profile.UniqueId.HasValue) throw new Exception("Profile didn't have a UniqueId");
            _profileManager.SetCurrentProfile(profile.UniqueId.Value);
        }

        private async void DeleteProfile(object? param)
        {
            if (param == null) throw new ArgumentNullException(nameof(param));
            if (!(param is IBindingProfileViewModel)) throw new ArgumentException($"This command requires the type {nameof(IBindingProfileViewModel)}.");
            try
            {
                var profile = (IBindingProfileViewModel)param;
                if (profile.UniqueId == CurrentProfile?.UniqueId)
                {
                    throw new UserMessageException("Cannot delete currently selected profile.");
                }
                if (profile.UniqueId == null) throw new Exception($"Profile '{profile.Name}' doesn't have an id.");
            
                await _profileRepository.DeleteProfile(profile.UniqueId.Value);
                ErrorMessage = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting profile.");
                ErrorMessage = "Error deleting the profile.";
            }
        }

        private bool CanDeleteProfile(object? param)
        {
            if (param == null) return false;
            if (!(param is IBindingProfileViewModel)) return false;
            var profile = (IBindingProfileViewModel)param;
            return profile.UniqueId != CurrentProfile?.UniqueId;
        }

        public void ProfileManager_OnCurrentProfileChanged(object? source, ProfileManager.ProfileChangedEventArgs eventArgs)
        {
            CurrentProfile = ProfileList.Single(p => p.UniqueId == eventArgs.NewProfile.UniqueId);
            _commandDeleteProfile.FireCanExecuteChanged();
        }

        public bool IsLoaded
        {
            get => _isLoaded;
            private set
            {
                if (_isLoaded != value)
                {
                    _isLoaded = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsLoading));
                }
            }
        }
        public bool IsLoading => !IsLoaded;

        public IBindingProfileViewModel? CurrentProfile
        {
            get => _currentProfile;
            private set
            {
                if (_currentProfile != value)
                {
                    _currentProfile = value;
                    OnPropertyChanged();
                }
            }
        }
        public ObservableCollection<IBindingProfileViewModel> ProfileList { get; }

        public ICommand CommandDeleteProfile => _commandDeleteProfile;

        public ICommand CommandAddProfile { get; }

        public string? ErrorMessage
        {
            get => _errorMessage; 
            private set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
