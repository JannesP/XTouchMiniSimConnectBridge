using JannesP.Midi;
using JannesP.Midi.MidiProtocol;
using JannesP.Midi.Natives;
using JannesP.XTouchMini;
using JannesP.XTouchMini.Enums;
using System;
using System.Threading.Tasks;

namespace DirtyConsoleTests
{
    class Program
    {
        static async Task Main(string[] args)
        {
            //await MidiTests.Run();
            //await SimconnectTests.Run();
            await MidiControlsPlaneTests.Run();

            Console.WriteLine("Program ended! Exit with enter ...");
            Console.ReadLine();
        }

        
    }
}
