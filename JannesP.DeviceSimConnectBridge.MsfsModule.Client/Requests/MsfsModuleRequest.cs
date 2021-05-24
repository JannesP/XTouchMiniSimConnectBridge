using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.MsfsModule.Client.Requests
{
    public abstract class MsfsModuleRequest
    {
        public static string DataAreaModuleBaseName { get; } = "jannesp.device_simconnect_bridge";
        public static string DataAreaModuleInputBaseName { get; } = $"{DataAreaModuleBaseName}.module_input";
        public static string DataAreaModuleOutputBaseName { get; } = $"{DataAreaModuleBaseName}.module_output";
    }
}