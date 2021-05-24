using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.DeviceSimConnectBridge.MsfsModule.Client.Requests
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ReadLVarRequestData
    {
        public int RequestId;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string LVarName;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ReadLVarResponseData
    {
        public int RequestId;
        public double Value;
    }

    public class ReadLVarRequest : MsfsModuleRequest
    {
        public ReadLVarRequest(string lvarName)
        {
            if (string.IsNullOrWhiteSpace(lvarName)) throw new ArgumentException("Name cannot be empty.", nameof(lvarName));
            if (lvarName.Length > 128) throw new ArgumentException("Name cannot be longer than 128 characters.", nameof(lvarName));
            LVarName = lvarName;
        }

        public static uint DefineId { get; } = 1;
        public static uint RequestId { get; } = 1;
        public string LVarName { get; }

        private static MsfsModuleChannelDefinition DataAreaModuleInput { get; } = new MsfsModuleChannelDefinition($"{DataAreaModuleInputBaseName}.read_lvar", DefineId, (uint)Marshal.SizeOf<ReadLVarRequestData>());
        private static MsfsModuleChannelDefinition DataAreaModuleOutput { get; } = new MsfsModuleChannelDefinition($"{DataAreaModuleOutputBaseName}.read_lvar", DefineId + 100, (uint)Marshal.SizeOf<ReadLVarResponseData>());

        internal static void RegisterDataDefinition(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            simConnect.AddToClientDataDefinition<ReadLVarRequestData>(DefineId);
        }

        internal static void SetupChannels(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            simConnect.AddToClientDataDefinition<ReadLVarRequestData>(DefineId).Wait();
            simConnect.AddToClientDataDefinition<ReadLVarResponseData>(DefineId + 100).Wait();

            simConnect.MapClientDataNameToID(DataAreaModuleInput.Name, DataAreaModuleInput.Id).Wait();
            simConnect.MapClientDataNameToID(DataAreaModuleOutput.Name, DataAreaModuleOutput.Id).Wait();

            simConnect.RequestClientData(DataAreaModuleOutput.Id, RequestId, DefineId + 100, SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET).Wait();
        }

        internal async Task<double> Execute(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            ReadLVarRequestData data = new ReadLVarRequestData()
            {
                LVarName = LVarName,
            };
            await simConnect.SetClientData(DataAreaModuleInput.Id, DefineId, SIMCONNECT_CLIENT_DATA_SET_FLAG.DEFAULT, data);
            return 0.0d;
        }
    }
}