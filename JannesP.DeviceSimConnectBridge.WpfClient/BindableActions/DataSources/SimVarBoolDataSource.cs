using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using JannesP.SimConnectWrapper;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.DataSources
{
    [DataContract]
    public class SimVarBoolDataSource : ISimBoolSourceAction
    {
        private SimConnectManager? _simConnectManager;
        private string? _simVarName;
        private int? _interval;
        private int? _intervalId;
        private SimConnectDataDefinition? _dataDefinition;

        public string Name => "Retrieve Bool SimVar";
        public string Description => "Retrieves a SimVar with SimConnect that is a Bool (eg. 'AUTOPILOT MASTER').";
        public string UniqueIdentifier => nameof(SimVarBoolDataSource);
        public bool IsInitialized { get; private set; } = false;

        [DataMember]
        [StringActionSetting("SimVar Name", "The name of the SimVar (eg. \"AUTOPILOT MASTER\")", CanBeEmpty = false)]
        public string? SimVarName
        {
            get => _simVarName;
            set
            {
                if (_simVarName != value)
                {
                    _simVarName = value;
                    if (_simVarName != null)
                    {
                        _dataDefinition = new SimConnectDataDefinition(_simVarName, SimConnectDataDefinition.SimConnectUnitName.Bool, SimConnectDataType.FLOAT64);
                        _ = UpdateIntervalRequestAsync();
                    }
                    else
                    {
                        _dataDefinition = null;
                    }
                }
            }
        }

        [DataMember]
        [IntActionSetting("Interval", "The polling frequency in ms.", Min = 20, Max = 60000)]
        public int? Interval
        {
            get => _interval;
            set
            {
                if (_interval != value)
                {
                    _interval = value;
                }
            }
        }

        public event EventHandler<SimDataReceivedEventArgs<bool>>? SimBoolReceived;

        private async Task UpdateIntervalRequestAsync()
        {
            if (_intervalId.HasValue)
            {
                _simConnectManager?.SimConnectWrapper?.CancelIntervalRequest(_intervalId.Value).Wait();
            }
            if (_interval.HasValue && _dataDefinition != null)
            {
                if (_simConnectManager?.SimConnectWrapper != null)
                {
                    _simConnectManager.SimConnectWrapper.IntervalRequestResult -= SimConnectWrapper_IntervalRequestResult;
                    _simConnectManager.SimConnectWrapper.IntervalRequestResult += SimConnectWrapper_IntervalRequestResult;
                    _intervalId = await _simConnectManager.SimConnectWrapper.IntervalRequestObjectByType<double>(_interval.Value, _dataDefinition);
                }
            }
        }

        private void SimConnectWrapper_IntervalRequestResult(object? sender, SimConnectWrapper.EventArgs.IntervalRequestResultEventArgs e)
        {
            if (e.Result == null) return;
            SimBoolReceived?.Invoke(this, new SimDataReceivedEventArgs<bool>((double)e.Result != 0));
        }

        public void Deactivate() 
        {
            IsInitialized = false;
            if (_intervalId.HasValue)
            {
                _simConnectManager?.SimConnectWrapper?.CancelIntervalRequest(_intervalId.Value).Wait();
            }
            if (_simConnectManager != null)
            {
                _simConnectManager.StateChanged -= SimConnectManager_StateChanged;
            }
        }

        public async void Initialize(IServiceProvider serviceProvider)
        {
            IsInitialized = true;
            if (_simConnectManager == null)
            {
                _simConnectManager = serviceProvider.GetService<SimConnectManager>();
                if (_simConnectManager != null)
                {
                    await UpdateIntervalRequestAsync();
                    _simConnectManager.StateChanged += SimConnectManager_StateChanged;
                }
            }
        }

        private async void SimConnectManager_StateChanged(object? sender, SimConnectManager.StateChangedEventArgs e)
        {
            if (e.NewState == SimConnectManager.State.Connected && IsInitialized)
            {
                await UpdateIntervalRequestAsync();
            }
            else
            {
                _intervalId = null;
            }
        }


    }
}
