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

    public class ReadLVarRequest : MsfsModuleRequestWithResponse<double>
    {
        public const uint DefineId = 1;

        public const uint RequestId = 1;

        public ReadLVarRequest(string lvarName)
        {
            if (string.IsNullOrWhiteSpace(lvarName)) throw new ArgumentException("Name cannot be empty.", nameof(lvarName));
            if (lvarName.Length > 128) throw new ArgumentException("Name cannot be longer than 128 characters.", nameof(lvarName));
            LVarName = lvarName;
        }

        public string LVarName { get; }

        private static MsfsModuleChannelDefinition DataAreaModuleInput { get; } = new MsfsModuleChannelDefinition($"{DataAreaModuleInputBaseName}.read_lvar", DefineId, (uint)Marshal.SizeOf<ReadLVarRequestData>());
        private static MsfsModuleChannelDefinition DataAreaModuleOutput { get; } = new MsfsModuleChannelDefinition($"{DataAreaModuleOutputBaseName}.read_lvar", DefineId + 100, (uint)Marshal.SizeOf<ReadLVarResponseData>());

        internal static void HandleResponse(object[] responses)
        {
            var data = (ReadLVarResponseData)responses[0];
            FinishWithId(data.RequestId, data.Value);
        }

        internal static async Task RegisterDataDefinition(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            await simConnect.AddToClientDataDefinition<ReadLVarRequestData>(DefineId);
        }

        internal static async Task SetupChannels(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            await simConnect.AddToClientDataDefinition<ReadLVarRequestData>(DefineId).ConfigureAwait(false);
            await simConnect.AddToClientDataDefinition<ReadLVarResponseData>(DefineId + 100).ConfigureAwait(false);

            await simConnect.MapClientDataNameToID(DataAreaModuleInput.Name, DataAreaModuleInput.Id).ConfigureAwait(false);
            await simConnect.MapClientDataNameToID(DataAreaModuleOutput.Name, DataAreaModuleOutput.Id).ConfigureAwait(false);

            await simConnect.RequestClientData(DataAreaModuleOutput.Id, RequestId, DefineId + 100, SIMCONNECT_CLIENT_DATA_PERIOD.ON_SET).ConfigureAwait(false);
        }

        internal async Task<double> Execute(SimConnectWrapper.SimConnectWrapper simConnect)
        {
            var data = new ReadLVarRequestData()
            {
                RequestId = MyRequestId,
                LVarName = LVarName,
            };
            await simConnect.SetClientData(DataAreaModuleInput.Id, DefineId, SIMCONNECT_CLIENT_DATA_SET_FLAG.DEFAULT, data).ConfigureAwait(false);
            return await WaitForFinish(TimeSpan.FromSeconds(2)).ConfigureAwait(false);
        }

        internal override void Finish(object response)
        {
            var res = (ReadLVarResponseData)response;
            CompleteTask(res.Value);
        }
    }
}