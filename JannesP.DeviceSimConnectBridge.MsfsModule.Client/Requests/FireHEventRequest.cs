using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.DeviceSimConnectBridge.MsfsModule.Client.Requests
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FireHEventRequestData
    {
        public int RequestId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string HEventName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct FireHEventResponseData
    {
        public int RequestId;
    }

    public class FireHEventRequest : MsfsModuleRequest
    {
        public FireHEventRequest(string heventName)
        {
            if (string.IsNullOrWhiteSpace(heventName)) throw new ArgumentException("Name cannot be empty.", nameof(heventName));
            if (heventName.Length > 128) throw new ArgumentException("Name cannot be longer than 128 characters.", nameof(heventName));
            HEventName = heventName;
        }

        public static uint DefineId { get; } = 2;
        public static uint RequestId { get; } = 2;
        public string HEventName { get; }
        private static MsfsModuleChannelDefinition DataAreaModuleInput { get; } = new MsfsModuleChannelDefinition($"{DataAreaModuleInputBaseName}.fire_hevent", DefineId, (uint)Marshal.SizeOf<FireHEventRequestData>());
        private static MsfsModuleChannelDefinition DataAreaModuleOutput { get; } = new MsfsModuleChannelDefinition($"{DataAreaModuleOutputBaseName}.fire_hevent", DefineId + 100, (uint)Marshal.SizeOf<FireHEventResponseData>());

        internal static void SetupChannels(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            simConnect.AddToClientDataDefinition<FireHEventRequestData>(DefineId);
            simConnect.AddToClientDataDefinition<FireHEventResponseData>(DefineId + 100);

            simConnect.MapClientDataNameToID(DataAreaModuleInput.Name, DataAreaModuleInput.Id).Wait();
            simConnect.MapClientDataNameToID(DataAreaModuleOutput.Name, DataAreaModuleOutput.Id).Wait();

            simConnect.RequestClientData(DataAreaModuleOutput.Id, RequestId, DefineId + 100, SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET).Wait();
        }

        internal async Task Execute(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            var data = new FireHEventRequestData()
            {
                HEventName = HEventName,
            };
            await simConnect.SetClientData(DataAreaModuleInput.Id, DefineId, SIMCONNECT_CLIENT_DATA_SET_FLAG.DEFAULT, data);
        }
    }
}