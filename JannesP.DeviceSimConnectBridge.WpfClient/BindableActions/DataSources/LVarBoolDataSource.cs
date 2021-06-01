using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using System.Diagnostics.CodeAnalysis;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.DataSources
{
    [DataContract]
    public class LVarBoolDataSource : ISimBoolSourceAction
    {
        private readonly SemaphoreSlim _sem = new(1);
        private int? _interval;
        private int? _intervalId;
        private string? _lvarVarName;
        private SimConnectManager? _simConnectManager;

        public event EventHandler<SimDataReceivedEventArgs<bool>>? SimBoolReceived;

        public string Description => "Retrieves an LVar from MSFS that represents a Bool (eg. 'WT_CJ4_HDG_ON').";

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

        public bool IsInitialized { get; private set; } = false;

        [DataMember]
        [StringActionSetting("LVar Name", "The name of the LVar (eg. \"WT_CJ4_HDG_ON\")", CanBeEmpty = false)]
        public string? LVarVarName
        {
            get => _lvarVarName;
            set
            {
                if (_lvarVarName != value)
                {
                    _lvarVarName = value;
                    if (!string.IsNullOrWhiteSpace(_lvarVarName))
                    {
                        _ = UpdateIntervalRequestAsync();
                    }
                    else
                    {
                        _lvarVarName = null;
                    }
                }
            }
        }

        public string Name => "Retrieve Bool LVar";
        public string UniqueIdentifier => nameof(LVarBoolDataSource);

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

        [MemberNotNullWhen(true, nameof(Interval), nameof(LVarVarName))]
        private bool AreSettingsValidInternal() => this.AreSettingsValid();

        private void MsfsClient_IntervalResult(object? sender, MsfsModule.Client.IntervalResultEventArgs e)
        {
            if (e.LVarName == LVarVarName)
            {
                SimBoolReceived?.Invoke(this, new SimDataReceivedEventArgs<bool>((double)e.Value != 0));
            }
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

        private void SimConnectWrapper_IntervalRequestResult(object? sender, SimConnectWrapper.EventArgs.IntervalRequestResultEventArgs e)
        {
            if (e.Result == null) return;
            SimBoolReceived?.Invoke(this, new SimDataReceivedEventArgs<bool>((double)e.Result != 0));
        }

        private async Task UpdateIntervalRequestAsync()
        {
            await _sem.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_intervalId.HasValue)
                {
                    if (_simConnectManager?.MsfsClient != null)
                    {
                        _simConnectManager.MsfsClient.StopLVarIntervalUpdates(_intervalId.Value);
                    }
                    _intervalId = null;
                }
                if (IsInitialized && AreSettingsValidInternal())
                {
                    if (_simConnectManager?.MsfsClient != null)
                    {
                        _simConnectManager.MsfsClient.IntervalResult -= MsfsClient_IntervalResult;
                        _simConnectManager.MsfsClient.IntervalResult += MsfsClient_IntervalResult;
                        _intervalId = _simConnectManager.MsfsClient.StartLVarIntervalUpdates(LVarVarName, TimeSpan.FromMilliseconds(Interval.Value));
                    }
                }
            }
            finally { _sem.Release(); }
        }
    }
}