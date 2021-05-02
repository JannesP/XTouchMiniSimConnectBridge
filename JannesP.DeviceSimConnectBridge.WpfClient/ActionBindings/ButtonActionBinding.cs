using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings
{
    public class ButtonActionBinding : ActionBinding
    {
        private ILogger<ButtonActionBinding>? _logger;

        public ISimpleBindableAction? ButtonPressed { get; set; }

        public bool TriggerOnRelease { get; set; } = false;

        public override void Disable()
        {
            if (Device != null)
            {
                Device.ButtonUp -= Device_ButtonUp;
                Device.ButtonDown -= Device_ButtonDown;
            }
            base.Disable();
        }

        public override void Enable(IServiceProvider serviceProvider, IDevice device)
        {
            base.Enable(serviceProvider, device);
            _logger = serviceProvider.GetRequiredService<ILogger<ButtonActionBinding>>();

            Device.ButtonDown += Device_ButtonDown;
            Device.ButtonUp += Device_ButtonUp;
        }

        private void Device_ButtonDown(object? sender, DeviceButtonEventArgs e)
        {
            if (!TriggerOnRelease)
            {
                TriggerAction(e);
            }
        }

        private void Device_ButtonUp(object? sender, DeviceButtonEventArgs e)
        {
            if (TriggerOnRelease)
            {
                TriggerAction(e);
            }
        }

        private async void TriggerAction(DeviceButtonEventArgs e)
        {
            if (base.ServiceProvider == null || base.Device == null)
            {
                _logger?.LogWarning("EncoderActionBinding triggered before it got enabled.");
                return;
            }
            if (e.Button.Id == base.DeviceControlId)
            {
                if (ButtonPressed != null)
                {
                    if (!ButtonPressed.IsInitialized) ButtonPressed.Initialize(ServiceProvider);
                    await ButtonPressed.ExecuteAsync();
                }
            }
        }
    }
}
