using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.FlightSimulator.SimConnect;

namespace JannesP.SimConnectWrapper
{
    /// <summary>
    /// Yes I hate this but some SimConnect functions are simply missing in the SimConnect .net wrapper.
    /// Otherwise I'd need to create a complete new native wrapper ... I'll take the one use of reflection.
    /// </summary>
    internal static class SimConnectNative
    {
        private static Lazy<FieldInfo> _simConnectHandleFieldInfo = new Lazy<FieldInfo>(() => typeof(SimConnect).GetField("hSimConnect", BindingFlags.NonPublic | BindingFlags.Instance));

        public static IntPtr GetSimConnectHandle(this SimConnect simConnect)
        {
            return (IntPtr)_simConnectHandleFieldInfo.Value.GetValue(simConnect);
        }

        [DllImport("SimConnect.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Ansi)]
        public static extern int SimConnect_GetLastSentPacketID(IntPtr hSimConnect, out uint dwSendID);
    }
}
