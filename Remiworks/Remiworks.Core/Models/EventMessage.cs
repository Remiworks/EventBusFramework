using System;

namespace Remiworks.Core.Models
{
    public class EventMessage
    {
        public string RoutingKey { get; set; }
        public Guid? CorrelationId { get; set; }
        public long? Timestamp { get; set; }
        public string ReplyQueueName { get; set; }
        public string Type { get; set; }
        public string JsonMessage { get; set; }
        public bool IsError { get; set; }
        public ulong DeliveryTag { get; set; }
    }
}