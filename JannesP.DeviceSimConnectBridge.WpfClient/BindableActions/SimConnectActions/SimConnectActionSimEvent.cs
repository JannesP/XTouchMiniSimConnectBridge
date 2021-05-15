using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.SimConnectActions
{
    [DataContract]
    internal class SimConnectActionSimEvent : ISimpleBindableAction
    {
        protected ILogger<SimConnectActionSimEvent>? _logger;
        protected SimConnectManager? _simConnectManager;

        public string Description => "Sends a simple Sim Event though SimConnect.";
        public bool IsInitialized { get; private set; }
        public string Name => "Send SimConnect Event";

        [DataMember]
        [StringActionSetting("Event Name", "You can find them in the MSFS docs. (for example 'HEADING_BUG_INC' and 'COM_RADIO_FRACT_DEC')", CanBeEmpty = false)]
        public string? SimConnectEventName { get; set; }

        public string UniqueIdentifier => nameof(SimConnectActionSimEvent);

        public Task DeactivateAsync()
        {
            IsInitialized = false;
            return Task.CompletedTask;
        }

        public async Task ExecuteAsync()
        {
            if (!IsInitialized) throw new InvalidOperationException("Actions need to be initialized before usage.");
            if (SimConnectEventName != null)
            {
                if (_simConnectManager == null)
                {
                    _logger?.LogWarning("Couldn't get SimConnectManager, this action ({0}) won't work.", SimConnectEventName);
                }
                else
                {
                    SimConnectWrapper.SimConnectWrapper? simConnect = _simConnectManager.SimConnectWrapper;
                    if (simConnect != null)
                    {
                        await simConnect.SendEvent(SimConnectEventName).ConfigureAwait(false);
                    }
                }
            }
        }

        public Task InitializeAsync(IServiceProvider serviceProvider)
        {
            if (_logger == null) _logger = serviceProvider.GetRequiredService<ILogger<SimConnectActionSimEvent>>();
            if (_simConnectManager == null) _simConnectManager = serviceProvider.GetService<SimConnectManager>();
            IsInitialized = true;
            return Task.CompletedTask;
        }
    }
}