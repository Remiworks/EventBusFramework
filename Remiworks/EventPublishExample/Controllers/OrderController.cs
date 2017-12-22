using System.Collections.Generic;
using System.Threading.Tasks;
using EventPublishExample.Events;
using Remiworks.Core.Event.Publisher;

namespace EventPublishExample.Controllers
{
    public class OrderController
    {
        private readonly IEventPublisher _eventPublisher;

        public OrderController(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public async Task PlaceOrder(string deliveryAddress, decimal totalPrice, List<string> products)
        {
            // Do something to place the order here

            var orderPlacedEvent = new OrderPlacedEvent
            {
                DeliveryAddress = deliveryAddress,
                TotalPrice = totalPrice,
                Products = products
            };

            await _eventPublisher.SendEventAsync(orderPlacedEvent, "order.placed");
        }
    }
}