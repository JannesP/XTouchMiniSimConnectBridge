using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Runtime.Serialization;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.SimConnectActions
{
    [DataContract]
    internal class SimConnectActionSimEvent : ISimpleBindableAction
    {
        protected ILogger<SimConnectActionSimEvent>? _logger;
        protected SimConnectManager? _simConnectManager;

        public bool IsInitialized { get; private set; }
        public string Name => "Send SimConnect Event";
        public string Description => "Sends a simple Sim Event though SimConnect.";
        public string UniqueIdentifier => nameof(SimConnectActionSimEvent);

        [DataMember]
        [StringActionSetting("Event Name", "You can find them in the MSFS docs. (for example 'HEADING_BUG_INC' and 'COM_RADIO_FRACT_DEC')", CanBeEmpty = false)]
        public string? SimConnectEventName { get; set; }

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
            if (_simConnectManager == null ) _simConnectManager = serviceProvider.GetService<SimConnectManager>();
            IsInitialized = true;
            return Task.CompletedTask;
        }
    }
}
