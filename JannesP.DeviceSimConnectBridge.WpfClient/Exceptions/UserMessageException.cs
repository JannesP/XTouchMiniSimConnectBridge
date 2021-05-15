using System;
using System.Runtime.Serialization;

namespace JannesP.DeviceSimConnectBridge.WpfApp.Exceptions
{
    public class UserMessageException : Exception
    {
        public UserMessageException(string? userMessage)
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

        protected UserMessageException(SerializationInfo info, StreamingContext context, string? userMessage) : base(info, context)
        {
            UserMessage = userMessage;
        }

        public string? UserMessage { get; }
    }
}