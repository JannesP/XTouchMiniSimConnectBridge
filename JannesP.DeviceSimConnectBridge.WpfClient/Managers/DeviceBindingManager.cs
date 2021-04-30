using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using JannesP.DeviceSimConnectBridge.WpfApp.Options;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Managers
{
    internal class DeviceBindingManager
    {
        private readonly ILogger<DeviceBindingManager> _logger;
        private readonly DeviceRepository _deviceRepository;
        private readonly ProfileManager _profileManager;
        private readonly IServiceProvider _serviceProvider;

        public DeviceBindingManager(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<DeviceBindingManager>>();
            _deviceRepository = serviceProvider.GetRequiredService<DeviceRepository>();
            _profileManager = serviceProvider.GetRequiredService<ProfileManager>();

            _serviceProvider = serviceProvider;
        }

        public async Task Enable()
        {
            BindingProfile? profile = await _profileManager.GetCurrentProfileAsync().ConfigureAwait(false);
            if (profile.BindingConfigurations == null) throw new Exception("F");
            foreach (DeviceBindingConfiguration? bindingConfiguration in profile.BindingConfigurations)
            {
                if (bindingConfiguration.TechnicalDeviceIdentifier == null)
                {
                    _logger.LogWarning("A BindingConfiguration didn't have a {0} set.", nameof(DeviceBindingConfiguration.TechnicalDeviceIdentifier));
                    continue;
                }
                IDevice? device = _deviceRepository.FindDeviceByTechnicalIdentifier(bindingConfiguration.TechnicalDeviceIdentifier);
                if (device == null)
                {
                    _logger.LogInformation("Couldn't find device '{0}' for a BindingConfiguration.", bindingConfiguration.TechnicalDeviceIdentifier);
                    continue;
                }
                bindingConfiguration.Bindings.ForEach(ab => ab.Enable(_serviceProvider, device));
            }
        }
    }
}
