using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings
{
    [DataContract]
    public class LedActionBinding : ActionBinding
    {
        private ISimBoolSourceAction? _dataSource;
        private IDeviceLed? _deviceLed;
        private ILogger<LedActionBinding>? _logger;

        [DataMember]
        public ISimBoolSourceAction? DataSource
        {
            get => _dataSource; set
            {
                if (value != _dataSource)
                {
                    if (_dataSource != null)
                    {
                        _dataSource.SimBoolReceived -= DataSource_SimBoolReceived;
                    }
                    _dataSource = value;
                    if (_dataSource != null && IsEnabled && ServiceProvider != null)
                    {
                        _dataSource.SimBoolReceived += DataSource_SimBoolReceived;
                        _dataSource.InitializeAsync(ServiceProvider);
                    }
                }
            }
        }

        public override bool IsEmpty => DataSource == null;

        public override async void Disable()
        {
            base.Disable();
            if (DataSource != null)
            {
                DataSource.SimBoolReceived -= DataSource_SimBoolReceived;
                await DataSource.DeactivateAsync().ConfigureAwait(false);
            }
        }

        public override async void Enable(IServiceProvider serviceProvider, IDevice device)
        {
            bool wasEnabled = IsEnabled;
            base.Enable(serviceProvider, device);
            if (!wasEnabled)
            {
                if (_logger == null) _logger = serviceProvider.GetService<ILogger<LedActionBinding>>();
                if (DataSource != null)
                {
                    await DataSource.InitializeAsync(serviceProvider).ConfigureAwait(false);
                    DataSource.SimBoolReceived += DataSource_SimBoolReceived;
                }
            }
        }

        private void DataSource_SimBoolReceived(object? sender, SimDataReceivedEventArgs<bool> e)
        {
            if (DeviceControlId != null && Device != null)
            {
                if (_deviceLed == null)
                {
                    _deviceLed = Device.Leds.First(led => led.Id == DeviceControlId);
                }
                Device.SetLedState(_deviceLed, e.NewValue ? DeviceLedState.On : DeviceLedState.Off);
            }
            else
            {
                _deviceLed = null;
            }
        }
    }
}