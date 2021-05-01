using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Exceptions;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Repositories
{
    public class ProfileRepository
    {
        private readonly SemaphoreSlim _semProfiles = new(1);
        private readonly DirectoryInfo _directoryInfo = new(Constants.ProfileDirectory);
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.All,
            Formatting = Formatting.Indented,
        };

        private readonly ILogger<ProfileRepository> _logger;

        private List<string>? _profileLoadErrors = null;
        private List<BindingProfile>? _profiles = null;

        public event EventHandler? ProfilesLoaded;
        public event EventHandler<ProfileEventArgs>? ProfileAdded;
        public event EventHandler<ProfileEventArgs>? ProfileRemoved;

        public ProfileRepository(ILogger<ProfileRepository> logger)
        {
            _logger = logger;
        }

        public bool IsLoaded { get; private set; } = false;
        public IReadOnlyCollection<string>? ProfileLoadErrors => _profileLoadErrors;

        /// <summary>
        /// Loads all *.profile.json profiles from the subfolder.
        /// </summary>
        /// <exception cref="UserMessageException"></exception>
        /// <returns>The list of profiles.</returns>
        public List<BindingProfile> GetProfiles()
        {
            List<BindingProfile> result;
            _semProfiles.Wait();
            try
            {
                EnsureProfilesLoaded();
                result = _profiles;
            }
            finally
            {
                _semProfiles.Release();
            }
            return result;
        }

        public void AddProfile(BindingProfile profile)
        {
            _semProfiles.Wait();
            try
            {
                EnsureProfilesLoaded();
                if (_profiles.Any(p => p.FileName == profile.FileName || p.Name == profile.Name))
                {
                    throw new UserMessageException("A profile with the same filename or name already exists.");
                }
                _profiles.Add(profile);
            }
            finally
            {
                _semProfiles.Release();
            }
            ProfileAdded?.Invoke(profile, new ProfileEventArgs(profile));
        }

        public async Task DeleteProfile(Guid profileId)
        {
            BindingProfile bp;
            _semProfiles.Wait();
            try
            {
                EnsureProfilesLoaded();
                bp = _profiles.Single(p => p.UniqueId == profileId);
                if (bp.FileName == null) throw new Exception($"Profile with id {profileId} didn't have a FileName");
                string filePath = Path.Combine(_directoryInfo.FullName, $"{bp.FileName}.profile.json");
                await Task.Run(() =>
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);
                    }
                });
                _profiles.Remove(bp);
            }
            finally
            {
                _semProfiles.Release();
            }
            ProfileRemoved?.Invoke(this, new ProfileEventArgs(bp));
        }

        [MemberNotNull(nameof(_profiles))]
        private void EnsureProfilesLoaded()
        {
            if (_profiles == null) throw new InvalidOperationException("Profiles aren't loaded yet. Please call 'Task LoadProfilesAsync()' first.");
        }

        /// <summary>
        /// Preloads all *.profile.json profiles from the subfolder.
        /// </summary>
        public async Task LoadProfilesAsync()
        {
            await _semProfiles.WaitAsync().ConfigureAwait(false);
            try
            {
                _profiles = await UnsafeLoadProfilesAsync().ConfigureAwait(false);
            }
            finally
            {
                _semProfiles.Release();
            }
            
        }
        
        public async Task PersistProfilesAsync()
        {
            await _semProfiles.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_profiles != null)
                {
                    await Task.Run(async () =>
                    {
                        EnsureDirectoryExists();
                        foreach (BindingProfile profile in _profiles)
                        {
                            try
                            {
                                var file = new FileInfo(Path.Combine(_directoryInfo.FullName, $"{profile.FileName}.profile.json"));
                                await File.WriteAllTextAsync(file.FullName, JsonConvert.SerializeObject(profile, _serializerSettings)).ConfigureAwait(false);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogError(ex, "Error saving profile to file.");
                            }
                        }
                    }).ConfigureAwait(false);
                }
            }
            finally
            {
                _semProfiles.Release();
            }
        }

        private void EnsureDirectoryExists()
        {
            if (!_directoryInfo.Exists)
            {
                _directoryInfo.Create();
            }
        }

        private async Task<List<BindingProfile>> UnsafeLoadProfilesAsync()
        {
            if (_profiles == null)
            {
                var profileLoadErrors = new List<string>();
                var profiles = new List<BindingProfile>();
                await Task.Run(async () =>
                {
                    EnsureDirectoryExists();
                    FileInfo[] files = _directoryInfo.GetFiles($"*{Constants.ProfileFileEnding}", SearchOption.TopDirectoryOnly);
                    foreach (FileInfo? file in files)
                    {
                        try
                        {
                            string profileText = await File.ReadAllTextAsync(file.FullName).ConfigureAwait(false);
                            BindingProfile? profile = null;
                            try
                            {
                                profile = JsonConvert.DeserializeObject<BindingProfile>(profileText, _serializerSettings);
                                if (profile == null)
                                {
                                    profileLoadErrors.Add($"Error loading profile from '{file.FullName}'. DeserializeObject returned null.");
                                }
                                else
                                {
                                    profile.FileName = file.Name.Substring(0, file.Name.Length - Constants.ProfileFileEnding.Length);
                                    profiles.Add(profile);
                                }
                            }
                            catch (JsonSerializationException ex)
                            {
                                _logger.LogError(ex, "Json error in {0}.", file.FullName);
                                profileLoadErrors.Add($"The json in {file.FullName} is invalid: {ex.Message}");
                            }
                        }
                        catch (Exception ex)
                        {
                            profileLoadErrors.Add($"Uncaught error while loading profile from '{file.FullName}'. See the log for more info.");
                            _logger.LogError(ex, "Error loading profile from {0}.", file.FullName);
                        }
                    }
                }).ConfigureAwait(false);
                _profileLoadErrors = profileLoadErrors;
                _profiles = profiles;
                IsLoaded = true;

                //run in task to avoid deadlocks from consumers
                _ = Task.Run(() =>
                {
                    try
                    {
                        ProfilesLoaded?.Invoke(this, EventArgs.Empty);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Error in ProfilesLoaded callback.");
                    }
                });
            }
            return _profiles;
        }

        public class ProfileEventArgs : EventArgs
        {
            public BindingProfile Profile { get; }

            public ProfileEventArgs(BindingProfile profile)
            {
                Profile = profile;
            }
        }
    }
}
