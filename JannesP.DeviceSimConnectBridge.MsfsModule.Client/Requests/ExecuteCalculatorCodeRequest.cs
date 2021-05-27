using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.DeviceSimConnectBridge.MsfsModule.Client.Requests
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ExecuteCalculatorCodeRequestData
    {
        public int RequestId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string HEventName;
    }

    public class ExecuteCalculatorCodeRequest : MsfsModuleRequest
    {
        public ExecuteCalculatorCodeRequest(string heventName)
        {
            if (string.IsNullOrWhiteSpace(heventName)) throw new ArgumentException("Name cannot be empty.", nameof(heventName));
            if (heventName.Length > 128) throw new ArgumentException("Name cannot be longer than 128 characters.", nameof(heventName));
            HEventName = heventName;
        }

        public static uint DefineId { get; } = 2;
        public string HEventName { get; }
        private static MsfsModuleChannelDefinition DataAreaModuleInput { get; } = new MsfsModuleChannelDefinition($"{DataAreaModuleInputBaseName}.fire_hevent", DefineId, (uint)Marshal.SizeOf<ExecuteCalculatorCodeRequestData>());

        internal static async Task SetupChannels(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            await simConnect.AddToClientDataDefinition<ExecuteCalculatorCodeRequestData>(DefineId).ConfigureAwait(false);
            await simConnect.MapClientDataNameToID(DataAreaModuleInput.Name, DataAreaModuleInput.Id).ConfigureAwait(false);
        }

        internal async Task Execute(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            var data = new ExecuteCalculatorCodeRequestData()
            {
                RequestId = NextRequestId,
                HEventName = HEventName,
            };
            await simConnect.SetClientData(DataAreaModuleInput.Id, DefineId, SIMCONNECT_CLIENT_DATA_SET_FLAG.DEFAULT, data).ConfigureAwait(false);
        }
    }
}