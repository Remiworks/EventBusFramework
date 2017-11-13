using System;

namespace RabbitFramework
{
    public class RabbitInitializer
    {
        private IBusProvider _busProvider;

        public RabbitInitializer(IBusProvider busProvider)
        {
            _busProvider = busProvider;
        }

        public void Initialize()
        {
            _busProvider.CreateConnection();
        }

        public void RegisterQueueToCallback(string queueName)
        {
            _busProvider.BasicConsume(queueName, RunCallback);
        }

        public void RunCallback(EventMessage message)
        {

        }
    }
}