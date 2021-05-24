using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.DeviceSimConnectBridge.MsfsModule.Client;
using JannesP.SimConnectWrapper;

namespace DirtyConsoleTests
{
    internal static class MsfsModuleTests
    {
        public static async Task Run()
        {
            using (var simConnect = new SimConnectWrapper("MsfsModuleTests"))
            {
                Console.WriteLine("Connecting to SimConnect ...");
                if (!await simConnect.TryConnect())
                {
                    Console.WriteLine("SimConnect connection failed. Exiting ...");
                    return;
                }
                int x = 0;
                MsfsModuleClient client = new(simConnect);
                while (Console.ReadLine() != "stop")
                {
                    if (x++ % 2 == 0)
                    {
                        Console.WriteLine("Reading LVar WT_CJ4_HDG_ON ...");
                        double result = await client.ReadLVar("WT_CJ4_HDG_ON");
                        Console.WriteLine($"Response: {result}");
                    }
                    else
                    {
                        Console.WriteLine("Firing HEvent WT_CJ4_AP_HDG_PRESSED ...");
                        await client.FireHEvent("WT_CJ4_AP_HDG_PRESSED");
                    }
                }
                Console.WriteLine("stop typed, exiting ...");
                return;
            }
        }
    }
}