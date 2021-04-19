using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.XTouchMiniSimConnectBridge.WpfApp.Logging
{
    [EventSource(Name = "XTouchMiniSimConnectBridge")]
    internal class ApplicationEventSource : EventSource
    {
        public static readonly ApplicationEventSource Log = new();
        public static class Keywords
        {
            public const EventKeywords Tracing = (EventKeywords)(1 << 0);
            public const EventKeywords Lifecycle = (EventKeywords)(1 << 1);
        }

        [Event(1, Message = "Application starting.", Level = EventLevel.Informational, Keywords = Keywords.Lifecycle, Channel = EventChannel.Debug)]
        public void Startup() => WriteEvent(1);
        [Event(2, Message = "Second instance start detected.", Level = EventLevel.Informational, Keywords = Keywords.Lifecycle, Channel = EventChannel.Debug)]
        public void SecondInstanceStarted() => WriteEvent(2);
        [Event(3, Message = "We are not the first instance, shutting down.", Level = EventLevel.Informational, Keywords = Keywords.Lifecycle, Channel = EventChannel.Debug)]
        public void NotFirstInstance() => WriteEvent(3);

        [NonEvent]
        public void FatalException(Exception ex) => FatalException(ex.ToString());
        [Event(100, Message = "Fatal Exception occured, application is exiting:\n{0}", Level = EventLevel.Critical, Keywords = Keywords.Lifecycle, Channel = EventChannel.Debug)]
        public void FatalException(string ex) => WriteEvent(100, ex);
        [NonEvent]
        public void DisposeException(Exception ex) => DisposeException(ex.ToString());
        [Event(101, Message = "Dispose Exception occured:\n{0}", Level = EventLevel.Error, Keywords = Keywords.Tracing, Channel = EventChannel.Debug)]
        public void DisposeException(string exception) => WriteEvent(101, exception);


        [Event(1000, Level = EventLevel.Informational, Keywords = Keywords.Tracing, Channel = EventChannel.Debug)]
        public void Info(string message) => WriteEvent(1000, message);
        [Event(1001, Level = EventLevel.Warning, Keywords = Keywords.Tracing, Channel = EventChannel.Debug)]
        public void Warning(string message) => WriteEvent(1001, message);
        

    }
}
