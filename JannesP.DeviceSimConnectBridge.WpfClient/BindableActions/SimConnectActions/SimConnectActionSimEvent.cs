using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.SimConnectActions
{
    internal class SimConnectActionSimEvent : ISimpleBindableAction
    {
        protected ILogger<SimConnectActionSimEvent>? _logger;
        protected SimConnectManager? _simConnectManager;

        [JsonIgnore]
        public bool IsInitialized { get; private set; }

        public string Name => "Send SimConnect Event";
        public string Description => "Sends a simple Sim Event though SimConnect.";

        [StringActionSetting("Event Name (eg. 'HEADING_BUG_INC')", CanBeEmpty = false)]
        public string? SimConnectEventName { get; set; }

        public async Task ExecuteAsync()
        {
            if (!IsInitialized) throw new InvalidOperationException("Actions need to be initialized before usage.");
            if (SimConnectEventName != null)
            {
                if (_simConnectManager == null)
                {
                    _logger?.LogWarning("Couldn't get SimConnectManager, this binding ({0}) won't work.", SimConnectEventName);
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

        public void Initialize(IServiceProvider serviceProvider) 
        {
            _logger = serviceProvider.GetRequiredService<ILogger<SimConnectActionSimEvent>>();
            _simConnectManager = serviceProvider.GetService<SimConnectManager>();
            IsInitialized = true;
        }
    }
}
