using System;
using System.Threading.Tasks;

namespace DirtyConsoleTests
{
    internal class Program
    {
        private static async Task Main(string[] _)
        {
            //await MidiTests.Run();
            //await SimconnectTests.Run();
            await MidiControlsPlaneTests.Run();

            Console.WriteLine("Program ended! Exit with enter ...");
            Console.ReadLine();
        }
    }
}