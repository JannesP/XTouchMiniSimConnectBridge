using System;
using System.Threading.Tasks;
using JannesP.XTouchMini;
using JannesP.XTouchMini.Enums;

namespace DirtyConsoleTests
{
    internal static class MidiTests
    {
        public static async Task Run()
        {
            var control = new XTouchMiniMcMode();
            control.ButtonDown += Control_ButtonDown;
            control.ButtonUp += Control_ButtonUp;
            control.EncoderTurned += Control_EncoderTurned;
            control.FaderMoved += Control_FaderMoved;

            //control.MessageReceived += Control_MessageReceived;
            if (!await control.OpenDeviceAsync())
            {
                Console.WriteLine($"Couldn't find {XTouchMiniMcMode.MidiDeviceName}!");
                Console.ReadLine();
                return;
            }

            control.SetButtonLed(XTouchMiniMcButton.ButtonTop1, McLedState.On);

            Console.WriteLine("Enter to close, mc for switch ...");
            string command = Console.ReadLine();
            while (command != "")
            {
                command = Console.ReadLine();
            }

            await control.CloseDeviceAsync();

            Console.WriteLine("Enter to exit!");
            Console.ReadLine();
        }

        private static void Control_ButtonDown(object sender, JannesP.XTouchMini.EventArgs.XTouchMiniMcModeButtonEventArgs e)
        {
            Console.WriteLine("[Control_ButtonDown] {0}: DOWN", e.Control.Name);
        }

        private static void Control_ButtonUp(object sender, JannesP.XTouchMini.EventArgs.XTouchMiniMcModeButtonEventArgs e)
        {
            Console.WriteLine("[Control_ButtonUp] {0}: UP", e.Control.Name);
        }

        private static void Control_EncoderTurned(object sender, JannesP.XTouchMini.EventArgs.XTouchMiniMcModeEncoderTurnedEventArgs e)
        {
            Console.WriteLine("[Control_EncoderTurned] {0}: {1}", e.Control.Name, e.Ticks);
        }

        private static void Control_FaderMoved(object sender, JannesP.XTouchMini.EventArgs.XTouchMiniMcModeFaderMovedEventArgs e)
        {
            Console.WriteLine("[Control_FaderMoved] {0}: {1}", e.Control.Name, e.Value);
        }
    }
}