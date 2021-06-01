using System;
using System.Runtime.Serialization;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings
{
    [DataContract]
    public class EncoderActionBinding : ActionBinding
    {
        private ILogger<EncoderActionBinding>? _logger;

        [DataMember]
        public bool IgnoreSpeed { get; set; } = false;

        [DataMember]
        public ISimpleBindableAction? TurnAntiClockwise { get; set; }

        [DataMember]
        public ISimpleBindableAction? TurnClockwise { get; set; }

        public override void Disable()
        {
            if (Device != null)
            {
                Device.EncoderTurned -= Device_EncoderTurned;
            }
            base.Disable();
        }

        public override void Enable(IServiceProvider serviceProvider, IDevice device)
        {
            bool wasEnabled = IsEnabled;
            base.Enable(serviceProvider, device);
            if (!wasEnabled)
            {
                _logger = serviceProvider.GetRequiredService<ILogger<EncoderActionBinding>>();
                if (Device == null) throw new ArgumentException("device cannot be null here.", nameof(device));
                if (ServiceProvider == null) throw new ArgumentException("serviceProvider cannot be null here.", nameof(serviceProvider));
                Device.EncoderTurned += Device_EncoderTurned;
            }
        }

        public override bool IsEmpty() => TurnClockwise == null && TurnAntiClockwise == null;

        private async void Device_EncoderTurned(object? sender, DeviceEncoderEventArgs e)
        {
            if (base.ServiceProvider == null || base.Device == null)
            {
                _logger?.LogWarning("EncoderActionBinding triggered before it got enabled.");
                return;
            }

            if (e.Encoder.Id == base.DeviceControlId)
            {
                ISimpleBindableAction? action = null;
                if (e.Steps > 0)
                {
                    action = TurnClockwise;
                }
                else if (e.Steps < 0)
                {
                    action = TurnAntiClockwise;
                }

                if (action != null)
                {
                    if (!action.IsInitialized) await action.InitializeAsync(base.ServiceProvider).ConfigureAwait(false);
                    if (IgnoreSpeed)
                    {
                        await action.ExecuteAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        for (int remaining = Math.Abs(e.Steps); remaining > 0; remaining--)
                        {
                            await action.ExecuteAsync().ConfigureAwait(false);
                        }
                    }
                }
            }
        }
    }
}