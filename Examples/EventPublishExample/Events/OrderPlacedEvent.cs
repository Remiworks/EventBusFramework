using System.Collections.Generic;

namespace EventPublishExample.Events
{
    public class OrderPlacedEvent
    {
        public string DeliveryAddress { get; set; }
        public decimal TotalPrice { get; set; }
        public List<string> Products { get; set; }
    }
}