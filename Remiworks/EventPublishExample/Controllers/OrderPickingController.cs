using System;
using EventPublishExample.Events;
using Remiworks.Attributes;

namespace EventPublishExample.Controllers
{
    [QueueListener("OrderQueue")]
    public class OrderPickingController
    {
        [Event("order.placed")]
        public void HandleOrderPlacedEvent(OrderPlacedEvent orderPlacedEvent)
        {
            // Do something to handle event here

            Console.WriteLine($"Address: {orderPlacedEvent.DeliveryAddress}");
            Console.WriteLine($"Total price: {orderPlacedEvent.TotalPrice}");
            Console.WriteLine("Products:");
            orderPlacedEvent.Products.ForEach(productName => Console.WriteLine(productName));
        }
    }
}