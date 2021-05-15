using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.Device;
using JannesP.DeviceSimConnectBridge.WpfApp.BindableActions;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp.ActionBindings
{
    [DataContract]
    public class LedActionBinding : ActionBinding
    {
        private ILogger<LedActionBinding>? _logger;
        private ISimBoolSourceAction? _dataSource;
        private IDeviceLed? _deviceLed;
        private bool _isEnabled = false;

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

        public override async void Enable(IServiceProvider serviceProvider, IDevice device)
        {
            if (_isEnabled) throw new InvalidOperationException("LedActionBinding is already enabled!"); 
            base.Enable(serviceProvider, device);
            _isEnabled = true;
            
            if (_logger == null) _logger = serviceProvider.GetService<ILogger<LedActionBinding>>();
            if (DataSource != null)
            {
                await DataSource.InitializeAsync(serviceProvider).ConfigureAwait(false);
                DataSource.SimBoolReceived += DataSource_SimBoolReceived;
            }
        }

        public override async void Disable()
        {
            _isEnabled = false;
            base.Disable();
            if (DataSource != null)
            {
                DataSource.SimBoolReceived -= DataSource_SimBoolReceived;
                await DataSource.DeactivateAsync().ConfigureAwait(false);
            }
        }

        public override bool IsEmpty() => DataSource == null;
    }
}
