using System;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Managers
{
    internal class DeviceBindingManager
    {
        private readonly DeviceRepository _deviceRepository;
        private readonly ILogger<DeviceBindingManager> _logger;
        private readonly ProfileManager _profileManager;
        private readonly IServiceProvider _serviceProvider;

        public DeviceBindingManager(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<DeviceBindingManager>>();
            _deviceRepository = serviceProvider.GetRequiredService<DeviceRepository>();
            _profileManager = serviceProvider.GetRequiredService<ProfileManager>();

            _serviceProvider = serviceProvider;
        }

        public void Enable()
        {
            BindingProfile? profile = _profileManager.GetCurrentProfile();
            if (profile.BindingConfigurations == null) throw new Exception("F");
            foreach (DeviceBindingConfiguration? bindingConfiguration in profile.BindingConfigurations)
            {
                if (bindingConfiguration.DeviceType == null)
                {
                    _logger.LogWarning("A BindingConfiguration didn't have a {0} set.", nameof(DeviceBindingConfiguration.DeviceType));
                    continue;
                }
                IDevice? device = _deviceRepository.TryFindDevice(bindingConfiguration.DeviceType, bindingConfiguration.DeviceId);
                if (device == null)
                {
                    _logger.LogInformation("Couldn't find device '{0}:{1}' for a BindingConfiguration.", bindingConfiguration.DeviceType, bindingConfiguration.DeviceId ?? "<null>");
                    continue;
                }
                device.ConnectAsync();
                bindingConfiguration.Bindings.ForEach(ab => ab.Enable(_serviceProvider, device));
            }
        }
    }
}