using System;
using System.Threading.Tasks;
using JannesP.SimConnectWrapper;

namespace DirtyConsoleTests
{
    internal static class SimconnectTests
    {
        public static async Task Run()
        {
            using (var sc = new SimConnectWrapper("XTouchMiniBridge"))
            {
                sc.SimConnectOpen += Sc_SimConnectOpen;
                sc.SimConnectClose += Sc_SimConnectClose;
                if (await sc.TryConnect())
                {
                    Console.WriteLine("Close with empty enter ...");
                    while (Console.ReadLine() != "")
                    {
                        try
                        {
                            double data = await sc.RequestObjectByType<double>(SimDataDefinitions.PlaneHeadingDegreesMagnetic);
                            Console.WriteLine($"[Data Received] {data} {SimDataDefinitions.PlaneHeadingDegreesMagnetic.UnitName}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Data Error] {ex}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine("Couldn't connect to SimConnect!");
                }
            }
            Console.WriteLine("Close with enter 2 ...");
            Console.ReadLine();
        }

        private static void Sc_SimConnectClose(object sender, EventArgs e)
        {
            Console.WriteLine("SimConnect disconnected!");
        }

        private static void Sc_SimConnectOpen(object sender, EventArgs e)
        {
        }

        public static class SimDataDefinitions
        {
            public static SimConnectDataDefinition PlaneHeadingDegreesMagnetic = new("PLANE HEADING DEGREES MAGNETIC", "degrees", SimConnectDataType.FLOAT64);
        }

        public class SimEvent
        {
            public static SimEvent AP_ALT_VAR_DEC = new(_kEY_ID_MIN + 357, "AP_ALT_VAR_DEC", "Decrements reference altitude");
            public static SimEvent AP_ALT_VAR_INC = new(_kEY_ID_MIN + 356, "AP_ALT_VAR_INC", "Increments reference altitude");
            public static SimEvent AP_SPD_VAR_DEC = new(_kEY_ID_MIN + 357, "AP_SPD_VAR_DEC", "Decrements airspeed hold reference");
            public static SimEvent AP_SPD_VAR_INC = new(_kEY_ID_MIN + 356, "AP_SPD_VAR_INC", "Increments airspeed hold reference");
            public static SimEvent AP_VS_VAR_DEC = new(_kEY_ID_MIN + 357, "AP_VS_VAR_DEC", "Decrements vertical speed reference");
            public static SimEvent AP_VS_VAR_INC = new(_kEY_ID_MIN + 356, "AP_VS_VAR_INC", "Increments vertical speed reference");
            public static SimEvent HEADING_BUG_DEC = new(_kEY_ID_MIN + 357, "HEADING_BUG_DEC", "Decrements heading hold reference bug");
            public static SimEvent HEADING_BUG_INC = new(_kEY_ID_MIN + 356, "HEADING_BUG_INC", "Increments heading hold reference bug");
            private static readonly int _kEY_ID_MIN = 0x00010000;

            private SimEvent(int eventId, string name, string description)
            {
                EventId = eventId;
                Name = name;
                Description = description;
            }

            public string Description { get; }
            public int EventId { get; }
            public string Name { get; }
        }
    }
}