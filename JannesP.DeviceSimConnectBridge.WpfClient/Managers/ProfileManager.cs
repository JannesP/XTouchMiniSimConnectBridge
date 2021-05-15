using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Managers
{
    public class ProfileManager
    {
        private readonly ApplicationOptions _options;
        private readonly ProfileRepository _profileRepository;
        private readonly SemaphoreSlim _semProfile = new(1);

        public ProfileManager(ApplicationOptions options, ProfileRepository profileRepository)
        {
            _options = options;
            _profileRepository = profileRepository;
            GetCurrentProfile();
        }

        public event EventHandler<ProfileChangedEventArgs>? CurrentProfileChanged;

        public void CreateNewProfile(string name)
        {
            char[] fileNameInvalidChars = Path.GetInvalidFileNameChars();
            string fileName = new(name.Trim().ToLowerInvariant().Select(c => fileNameInvalidChars.Contains(c) ? '_' : c).ToArray());

            var profile = new BindingProfile()
            {
                UniqueId = Guid.NewGuid(),
                Name = name,
                FileName = fileName,
            };

            _profileRepository.AddProfile(profile);
        }

        public BindingProfile GetCurrentProfile()
        {
            BindingProfile result;
            bool profileChanged = false;
            _semProfile.Wait();
            try
            {
                List<BindingProfile> profiles = _profileRepository.GetProfiles();
                if (_options.CurrentProfileUniqueId == null)
                {
                    result = CreateSetDefaultProfile(profiles);
                    profileChanged = true;
                }
                else
                {
                    BindingProfile? profile = profiles.SingleOrDefault(p => p.UniqueId == _options.CurrentProfileUniqueId);
                    if (profile == null)
                    {
                        if (profiles.Count > 0)
                        {
                            profile = profiles.First();
                            _options.CurrentProfileUniqueId = profile.UniqueId;
                        }
                        else
                        {
                            profile = CreateSetDefaultProfile(profiles);
                        }
                        profileChanged = true;
                    }
                    result = profile;
                }
            }
            finally
            {
                _semProfile.Release();
            }
            if (profileChanged) OnCurrentProfileChanged(result);
            return result;
        }

        public void SetCurrentProfile(Guid uniqueId)
        {
            BindingProfile? profile = null;
            bool profileChanged = false;
            _semProfile.Wait();
            try
            {
                List<BindingProfile>? profiles = _profileRepository.GetProfiles();
                profile = profiles.Single(p => p.UniqueId == uniqueId);
                if (_options.CurrentProfileUniqueId != profile.UniqueId)
                {
                    _options.CurrentProfileUniqueId = profile.UniqueId;
                    profileChanged = true;
                }
            }
            finally
            {
                _semProfile.Release();
            }
            if (profileChanged && profile != null)
            {
                OnCurrentProfileChanged(profile);
            }
        }

        private BindingProfile CreateSetDefaultProfile(List<BindingProfile> profiles)
        {
            BindingProfile? defaultProfile = profiles.FirstOrDefault();
            if (defaultProfile == null)
            {
                defaultProfile = BindingProfile.CreateDefaultProfile();
                _profileRepository.AddProfile(defaultProfile);
            }
            _options.CurrentProfileUniqueId = defaultProfile.UniqueId;
            return defaultProfile;
        }

        private void OnCurrentProfileChanged(BindingProfile profile)
                            => CurrentProfileChanged?.Invoke(this, new ProfileChangedEventArgs(profile));

        public class ProfileChangedEventArgs : EventArgs
        {
            public ProfileChangedEventArgs(BindingProfile newProfile)
            {
                NewProfile = newProfile;
            }

            public BindingProfile NewProfile { get; }
        }
    }
}