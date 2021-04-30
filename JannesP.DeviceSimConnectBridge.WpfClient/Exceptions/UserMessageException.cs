using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Exceptions
{
    public class UserMessageException : Exception
    {
        public string? UserMessage { get; }

        public UserMessageException(string? userMessage)
        {
            UserMessage = userMessage;
        }

        protected UserMessageException(SerializationInfo info, StreamingContext context, string? userMessage) : base(info, context)
        {
            UserMessage = userMessage;
        }

        public UserMessageException(string? message, string? userMessage) : base(message)
        {
            UserMessage = userMessage;
        }

        public UserMessageException(string? message, Exception? innerException, string? userMessage) : base(message, innerException)
        {
            UserMessage = userMessage;
        }
    }
}
