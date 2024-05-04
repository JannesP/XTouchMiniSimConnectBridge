using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.SimConnectWrapper
{
    /// <summary>
    /// Yes I hate this but some SimConnect functions are simply missing in the SimConnect .net wrapper.
    /// Otherwise I'd need to create a complete new native wrapper ... I'll take the one use of reflection.
    /// </summary>
    internal static class SimConnectNative
    {
        private static readonly Lazy<FieldInfo> _simConnectHandleFieldInfo = new(() => typeof(SimConnect).GetField("hSimConnect", BindingFlags.NonPublic | BindingFlags.Instance) 
            ?? throw new Exception("Simconnect handle field couldn't be found."));

        public static IntPtr GetSimConnectHandle(this SimConnect simConnect)
        {
            object? result = _simConnectHandleFieldInfo.Value.GetValue(simConnect);
            if (result is IntPtr ptr)
            {
                return ptr;
            }
            throw new InvalidOperationException("SimConnectHandle was not set in the given SimConnect instance.");
        }

        [DllImport("SimConnect.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int SimConnect_GetLastSentPacketID(IntPtr hSimConnect, out uint dwSendID);
    }
}