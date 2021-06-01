using System;
using System.Runtime.Serialization;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings
{
    [DataContract]
    public class ButtonActionBinding : ActionBinding
    {
        private ILogger<ButtonActionBinding>? _logger;

        [DataMember]
        public ISimpleBindableAction? ButtonPressed { get; set; }

        [DataMember]
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
            bool wasEnabled = IsEnabled;
            base.Enable(serviceProvider, device);
            if (!wasEnabled)
            {
                _logger = serviceProvider.GetRequiredService<ILogger<ButtonActionBinding>>();

                Device.ButtonDown += Device_ButtonDown;
                Device.ButtonUp += Device_ButtonUp;
            }
        }

        public override bool IsEmpty() => ButtonPressed == null;

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
                    if (!ButtonPressed.IsInitialized) await ButtonPressed.InitializeAsync(ServiceProvider).ConfigureAwait(false);
                    await ButtonPressed.ExecuteAsync().ConfigureAwait(false);
                }
            }
        }
    }
}