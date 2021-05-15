using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
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
        public ISimpleBindableAction? TurnClockwise { get; set; }
        [DataMember]
        public ISimpleBindableAction? TurnAntiClockwise { get; set; }
        [DataMember]
        public bool IgnoreSpeed { get; set; } = false;

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
            base.Enable(serviceProvider, device);
            _logger = serviceProvider.GetRequiredService<ILogger<EncoderActionBinding>>();
            if (Device == null) throw new ArgumentException("device cannot be null here.", nameof(device));
            if (ServiceProvider == null) throw new ArgumentException("serviceProvider cannot be null here.", nameof(serviceProvider));
            Device.EncoderTurned += Device_EncoderTurned;
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
