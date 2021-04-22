using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JannesP.SimConnectWrapper;
using JannesP.XTouchMini;

namespace DirtyConsoleTests
{
    class MidiControlsPlaneTests
    {
        private static SimConnectWrapper _simConnect;
        private static XTouchMiniMcMode _control;
        private static int _intervalPlaneHeading;
        private static int _intervalApHeading;
        private static double _planeHeading;
        private static double _apHeading;

        public static class SimDataDefinitions
        {
            public static SimConnectDataDefinition PlaneHeadingDegreesMagnetic = new SimConnectDataDefinition(100, "PLANE HEADING DEGREES MAGNETIC", null, SimConnectDataType.FLOAT64);
            public static SimConnectDataDefinition AutopilotHeadingLockDir = new SimConnectDataDefinition(101, "AUTOPILOT HEADING LOCK DIR", null, SimConnectDataType.FLOAT64);
        }

        private static List<double> _ledRadians = new List<double>();

        public static async Task Run()
        {
            double start = 0.0d;
            //outermost leds are 133° to each side 133°=> 1.97222 rad
            double end = 1.97222d * 2;
            double step = (Math.Abs(start) + Math.Abs(end)) / 10d;
            for (int i = 0; i < 11; i++)
            {
                _ledRadians.Add(start + step * i);
            }
            //need to add the start at the end as well, otherwise everything above X is closest to the largest index
            _ledRadians.Add(Math.PI * 2.0d);

            using (_control = new XTouchMiniMcMode())
            {
                _control.ButtonDown += Control_ButtonDown;
                _control.ButtonUp += Control_ButtonUp;
                _control.EncoderTurned += Control_EncoderTurned;
                _control.FaderMoved += Control_FaderMoved;

                if (!await _control.OpenDeviceAsync())
                {
                    Console.WriteLine($"Couldn't find {XTouchMiniMcMode.MidiDeviceName}!");
                    Console.ReadLine();
                    return;
                }

                Console.WriteLine("[Midi] Opened XTouchMini Channels!");

                _control.SetEncoderLed(XTouchMiniMcEncoder.Encoder5, JannesP.XTouchMini.Enums.McEncoderRingStyle.Single, 0x01);
                _control.SetEncoderLed(XTouchMiniMcEncoder.Encoder6, JannesP.XTouchMini.Enums.McEncoderRingStyle.Fan, 0x0F);
                _control.SetEncoderLed(XTouchMiniMcEncoder.Encoder7, JannesP.XTouchMini.Enums.McEncoderRingStyle.Single, 0x0F);
                _control.SetEncoderLed(XTouchMiniMcEncoder.Encoder8, JannesP.XTouchMini.Enums.McEncoderRingStyle.Fan, 0x0F);
                try
                {

                    using (var simConnect = new SimConnectWrapper("XTouchMiniBridge"))
                    {
                        _simConnect = simConnect;
                        _simConnect.SimConnectOpen += _simConnect_SimConnectOpen;
                        _simConnect.IntervalRequestResult += _simConnect_IntervalRequestResult;
                        if (!await simConnect.TryConnect())
                        {
                            Console.WriteLine("Couldn't connect to SimConnect!");
                            return;
                        }

                        _intervalPlaneHeading = await _simConnect.IntervalRequestObjectByType<double>(50, SimDataDefinitions.PlaneHeadingDegreesMagnetic);
                        _intervalApHeading = await _simConnect.IntervalRequestObjectByType<double>(50, SimDataDefinitions.AutopilotHeadingLockDir);
                        Console.ReadLine();
                    }

                }
                finally 
                {
                    _simConnect = null;
                    await _control.CloseDeviceAsync();
                }
            }

            
        }

        private static async void _simConnect_SimConnectOpen(object sender, EventArgs e)
        {
            _planeHeading = await _simConnect.RequestObjectByType<double>(SimDataDefinitions.PlaneHeadingDegreesMagnetic);
            _apHeading = await _simConnect.RequestObjectByType<double>(SimDataDefinitions.AutopilotHeadingLockDir);
        }

        private static void _simConnect_IntervalRequestResult(object sender, JannesP.SimConnectWrapper.EventArgs.IntervalRequestResultEventArgs e)
        {
            if (e.Result == null) return;
            if (e.RequestId == _intervalPlaneHeading)
            {
                _planeHeading = (double)e.Result;
            }
            else if (e.RequestId == _intervalApHeading)
            {
                _apHeading = (double)e.Result;
            }
            UpdateHeadingLedRing();
        }

        private static void UpdateHeadingLedRing()
        {
            lock (_control)
            {
                double relativeHeading = _apHeading - _planeHeading;
                var lowestValue = double.MaxValue;
                int lowestIndex = 0;
                for (int i = 0; i < _ledRadians.Count; i++)
                {
                    double ledRadian = _ledRadians[i];
                    double normalizedRelative = relativeHeading + 1.97222d;
                    if (normalizedRelative > Math.PI * 2.0d)
                    {
                        normalizedRelative -= Math.PI * 2.0d;
                    }
                    if (normalizedRelative < 0.0d)
                    {
                        normalizedRelative += Math.PI * 2.0d;
                    }
                    double val = Math.Abs(normalizedRelative - ledRadian);
                    if (val < lowestValue)
                    {
                        lowestValue = val;
                        lowestIndex = i;
                    }
                }
                _control.SetEncoderLed(XTouchMiniMcEncoder.Encoder7, JannesP.XTouchMini.Enums.McEncoderRingStyle.Single, (byte)(0x01 * ((lowestIndex % 11) + 1)));
            }
        }

        private static void Control_FaderMoved(object sender, JannesP.XTouchMini.EventArgs.XTouchMiniMcModeFaderMovedEventArgs e)
        {
            Console.WriteLine("[Midi][Control_FaderMoved] {0}: {1}", e.Control.Name, e.Value);
        }

        private static async void Control_EncoderTurned(object sender, JannesP.XTouchMini.EventArgs.XTouchMiniMcModeEncoderTurnedEventArgs e)
        {
            Console.WriteLine("[Midi][Control_EncoderTurned] {0}: {1}", e.Control.Name, e.Ticks);
            string simEvent = null;
            if (e.Control == XTouchMiniMcEncoder.Encoder5)
            { 
                simEvent = e.Ticks > 0 ? SimconnectTests.SimEvent.AP_VS_VAR_INC.Name : SimconnectTests.SimEvent.AP_VS_VAR_DEC.Name;
            }
            if (e.Control == XTouchMiniMcEncoder.Encoder6)
            {
                simEvent = e.Ticks > 0 ? SimconnectTests.SimEvent.AP_SPD_VAR_INC.Name : SimconnectTests.SimEvent.AP_SPD_VAR_DEC.Name;
            }
            else if (e.Control == XTouchMiniMcEncoder.Encoder7)
            {
                simEvent = e.Ticks > 0 ? SimconnectTests.SimEvent.HEADING_BUG_INC.Name : SimconnectTests.SimEvent.HEADING_BUG_DEC.Name;
            }
            if (e.Control == XTouchMiniMcEncoder.Encoder8)
            {
                simEvent = e.Ticks > 0 ? SimconnectTests.SimEvent.AP_ALT_VAR_INC.Name : SimconnectTests.SimEvent.AP_ALT_VAR_DEC.Name;
            }
            if (simEvent != null)
            {
                for (var remaining = Math.Abs(e.Ticks); remaining > 0; remaining--)
                {
                    await _simConnect.SendEvent(simEvent);
                }
            }
        }

        private static void Control_ButtonUp(object sender, JannesP.XTouchMini.EventArgs.XTouchMiniMcModeButtonEventArgs e)
        {
            Console.WriteLine("[Midi][Control_ButtonUp] {0}: UP", e.Control.Name);
        }

        private static void Control_ButtonDown(object sender, JannesP.XTouchMini.EventArgs.XTouchMiniMcModeButtonEventArgs e)
        {
            Console.WriteLine("[Midi][Control_ButtonDown] {0}: DOWN", e.Control.Name);
        }

    }
}
