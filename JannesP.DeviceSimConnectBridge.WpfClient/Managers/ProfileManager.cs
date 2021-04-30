using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Managers
{
    public class ProfileManager
    {
        private readonly SemaphoreSlim _semProfile = new(1);
        private readonly ApplicationOptions _options;
        private readonly ProfileRepository _profileRepository;

        public event EventHandler<ProfileChangedEventArgs>? CurrentProfileChanged;

        public ProfileManager(ApplicationOptions options, ProfileRepository profileRepository)
        {
            _options = options;
            _profileRepository = profileRepository;
        }

        public async Task<BindingProfile> GetCurrentProfileAsync()
        {
            await _semProfile.WaitAsync().ConfigureAwait(false);
            try
            {
                BindingProfile result;
                List<BindingProfile> profiles = await _profileRepository.GetProfilesAsync().ConfigureAwait(false);
                if (_options.CurrentProfileUniqueId == null)
                {
                    result = await CreateSetDefaultProfileAsync(profiles).ConfigureAwait(false);
                    CurrentProfileChanged?.Invoke(this, new ProfileChangedEventArgs(result));
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
                            profile = await CreateSetDefaultProfileAsync(profiles).ConfigureAwait(false);
                        }
                        CurrentProfileChanged?.Invoke(this, new ProfileChangedEventArgs(profile));
                    }
                    result = profile;
                }
                return result;
            }
            finally
            {
                _semProfile.Release();
            }
            
        }

        private async Task<BindingProfile> CreateSetDefaultProfileAsync(List<BindingProfile> profiles)
        {
            BindingProfile? defaultProfile = profiles.FirstOrDefault();
            if (defaultProfile == null)
            {
                defaultProfile = BindingProfile.CreateDefaultProfile();
                await _profileRepository.AddProfileAsync(defaultProfile).ConfigureAwait(false);
            }
            _options.CurrentProfileUniqueId = defaultProfile.UniqueId;
            return defaultProfile;
        }

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
