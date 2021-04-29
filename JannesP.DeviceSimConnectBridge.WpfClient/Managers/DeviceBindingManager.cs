using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.SimConnectActions;
using JannesP.DeviceSimConnectBridge.Device.XTouchMini;
using JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings;
using Microsoft.Extensions.Logging;
using JannesP.DeviceSimConnectBridge.WpfApp.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Managers
{
    internal class DeviceBindingManager
    {
        private class DeviceBindingConfiguration
        {
            public string? TechnicalDeviceIdentifier { get; set; }
            public List<ActionBinding> Bindings { get; set; } = new List<ActionBinding>();
        }

        private readonly List<DeviceBindingConfiguration> _actionBindings;
        private readonly ILogger<DeviceBindingManager> _logger;
        private readonly DeviceRepository _deviceRepository;
        private readonly IServiceProvider _serviceProvider;

        public DeviceBindingManager(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<DeviceBindingManager>>();
            _deviceRepository = serviceProvider.GetRequiredService<DeviceRepository>();

            _actionBindings = new List<DeviceBindingConfiguration>()
            {
                new DeviceBindingConfiguration()
                {
                    TechnicalDeviceIdentifier = "behringer_xtouch_mini",
                    Bindings = new List<ActionBinding>()
                    {
                        new EncoderActionBinding()
                        {
                            DeviceControlId = 0x16,
                            TurnClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "HEADING_BUG_INC",
                            },
                            TurnAntiClockwise = new SimConnectActionSimEvent()
                            {
                                SimConnectEventName = "HEADING_BUG_DEC",
                            },
                        },
                    }
                }
            };
            _serviceProvider = serviceProvider;
        }

        public void Enable()
        {
            foreach (DeviceBindingConfiguration? bindingConfiguration in _actionBindings)
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
