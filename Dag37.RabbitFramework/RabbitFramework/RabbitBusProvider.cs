using RabbitMQ.Client;
using System;

namespace RabbitFramework
{
    public class RabbitBusProvider : IBusProvider
    {
        public BusOptions BusOptions { get; }

        private IConnectionFactory _connectionFactory;

        public RabbitBusProvider(BusOptions busOptions)
        {
            BusOptions = busOptions;
        }

        public void BasicConsume(string queueName, EventReceivedCallback callback)
        {
            throw new NotImplementedException();
        }

        public void BasicPublish(EventMessage message)
        {
            throw new NotImplementedException();
        }

        public void CreateConnection()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}