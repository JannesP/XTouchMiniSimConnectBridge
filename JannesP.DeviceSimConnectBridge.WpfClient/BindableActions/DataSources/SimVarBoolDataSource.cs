using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using JannesP.SimConnectWrapper;
using Microsoft.Extensions.DependencyInjection;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.DataSources
{
    [DataContract]
    public class SimVarBoolDataSource : ISimBoolSourceAction
    {
        private readonly SemaphoreSlim _sem = new(1);
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
                    if (!string.IsNullOrWhiteSpace(_simVarName))
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
                    _ = UpdateIntervalRequestAsync();
                }
            }
        }

        [MemberNotNullWhen(true, nameof(Interval), nameof(SimVarName), nameof(_dataDefinition))]
        private bool AreSettingsValidInternal() => this.AreSettingsValid();

        public event EventHandler<SimDataReceivedEventArgs<bool>>? SimBoolReceived;

        private async Task UpdateIntervalRequestAsync()
        {
            await _sem.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_intervalId.HasValue)
                {
                    if (_simConnectManager?.SimConnectWrapper != null)
                    {
                        await _simConnectManager.SimConnectWrapper.CancelIntervalRequest(_intervalId.Value).ConfigureAwait(false);
                    }
                    _intervalId = null;
                }
                if (IsInitialized && AreSettingsValidInternal())
                {
                    if (_simConnectManager?.SimConnectWrapper != null)
                    {
                        _simConnectManager.SimConnectWrapper.IntervalRequestResult -= SimConnectWrapper_IntervalRequestResult;
                        _simConnectManager.SimConnectWrapper.IntervalRequestResult += SimConnectWrapper_IntervalRequestResult;
                        _intervalId = await _simConnectManager.SimConnectWrapper.IntervalRequestObjectByType<double>(Interval.Value, _dataDefinition).ConfigureAwait(false);
                    }
                }
            }
            finally { _sem.Release(); }
        }

        private void SimConnectWrapper_IntervalRequestResult(object? sender, SimConnectWrapper.EventArgs.IntervalRequestResultEventArgs e)
        {
            if (e.Result == null) return;
            SimBoolReceived?.Invoke(this, new SimDataReceivedEventArgs<bool>((double)e.Result != 0));
        }

        public async Task DeactivateAsync() 
        {
            await _sem.WaitAsync().ConfigureAwait(false);
            try
            {
                IsInitialized = false;
            }
            finally { _sem.Release(); }
            await UpdateIntervalRequestAsync().ConfigureAwait(false);
        }

        public async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            await _sem.WaitAsync().ConfigureAwait(false);
            try
            {
                IsInitialized = true;
                if (_simConnectManager == null)
                {
                    _simConnectManager = serviceProvider.GetService<SimConnectManager>();
                    if (_simConnectManager != null)
                    {
                        _simConnectManager.StateChanged += SimConnectManager_StateChanged;
                    }
                }
            }
            finally { _sem.Release(); }
            await UpdateIntervalRequestAsync().ConfigureAwait(false);
        }

        private async void SimConnectManager_StateChanged(object? sender, SimConnectManager.StateChangedEventArgs e)
        {
            if (e.NewState == SimConnectManager.State.Connected && IsInitialized)
            {
                await UpdateIntervalRequestAsync().ConfigureAwait(false);
            }
            else
            {
                _intervalId = null;
            }
        }

    }
}
