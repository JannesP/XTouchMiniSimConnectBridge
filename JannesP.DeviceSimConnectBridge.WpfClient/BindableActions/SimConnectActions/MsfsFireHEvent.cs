using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.MsfsModule.Client;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.SimConnectActions
{
    [DataContract]
    public class MsfsFireHEvent : ISimpleBindableAction
    {
        #region Persistable Settings

        [DataMember]
        [StringActionSetting("Event Name", "You can find them in the docs for the plane you want to map. (for example 'A320_Neo_FCU_SPEED_PULL' and 'WT_CJ4_AP_HDG_PRESSED')", CanBeEmpty = false)]
        public string? HEventName { get; set; }

        #endregion Persistable Settings

        protected ILogger<MsfsFireHEvent>? _logger;
        protected SimConnectManager? _simConnectManager;

        public string Description => "Sends an HEvent to MSFS.";

        public bool IsInitialized { get; private set; }

        public string Name => "Send HEvent";

        public string UniqueIdentifier => nameof(MsfsFireHEvent);

        public Task DeactivateAsync()
        {
            IsInitialized = false;
            return Task.CompletedTask;
        }

        public async Task ExecuteAsync()
        {
            if (!IsInitialized) throw new InvalidOperationException("Actions need to be initialized before usage.");
            if (HEventName != null)
            {
                if (_simConnectManager == null)
                {
                    _logger?.LogWarning("Couldn't get SimConnectManager, this action ({0}) won't work.", HEventName);
                }
                else
                {
                    MsfsModuleClient? msfsClient = _simConnectManager.MsfsClient;
                    if (msfsClient != null)
                    {
                        await msfsClient.FireHEvent(HEventName).ConfigureAwait(false);
                    }
                }
            }
        }

        public Task InitializeAsync(IServiceProvider serviceProvider)
        {
            if (_logger == null) _logger = serviceProvider.GetRequiredService<ILogger<MsfsFireHEvent>>();
            if (_simConnectManager == null) _simConnectManager = serviceProvider.GetService<SimConnectManager>();
            IsInitialized = true;
            return Task.CompletedTask;
        }
    }
}