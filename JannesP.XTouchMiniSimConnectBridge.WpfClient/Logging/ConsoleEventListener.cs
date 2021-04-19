using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.XTouchMiniSimConnectBridge.WpfApp.Logging
{
    internal class ConsoleEventListener : EventListener
    {
        public static readonly ConsoleEventListener Instance = new ConsoleEventListener();

        protected override void OnEventSourceCreated(EventSource eventSource)
        {
            Console.WriteLine($"{eventSource.Guid} | {eventSource.Name}");
            if (eventSource.Name == "XTouchMiniSimConnectBridge")
            {
                EnableEvents(eventSource, EventLevel.Informational);
            }
        }

        protected override void OnEventWritten(EventWrittenEventArgs eventData)
        {
            Console.WriteLine($"Event Fired: {eventData.Message}");
        }
    }
}
