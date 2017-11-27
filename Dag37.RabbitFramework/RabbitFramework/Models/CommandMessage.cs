using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitFramework.Models
{
    public class CommandMessage
    {
        public string RoutingKey { get; set; }
        public Guid? CorrelationId { get; set; }
        public long? Timestamp { get; set; }
        public string ReplyQueueName { get; set; }
        public string Type { get; set; }
        public string JsonMessage { get; set; }
        public object Content { get; set; }
        public bool IsError { get; set; }
        public Exception Exception { get; set; }
    }
}
