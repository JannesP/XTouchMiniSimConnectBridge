using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.WpfApp.Managers;

namespace JannesP.DeviceSimConnectBridge.WpfApp.BindableActions.SimConnectActions
{
    class SimConnectActionSimEvent : IBindableAction
    {
        protected SimConnectManager _simConnectManager;

        public SimConnectActionSimEvent(SimConnectManager simConnectManager)
        {
            _simConnectManager = simConnectManager;
        }

        public string Name => "Send SimConnect Event";
        public string Description => "Sends a simple Sim Event though SimConnect.";

        [StringActionSetting("Event Name (eg. 'HEADING_BUG_INC')", CanBeEmpty = false)]
        public string? SimConnectEventName { get; set; }

        public Task ExecuteAsync() => throw new NotImplementedException();
    }
}
