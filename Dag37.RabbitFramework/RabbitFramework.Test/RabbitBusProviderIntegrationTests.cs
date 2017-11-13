using Microsoft.VisualStudio.TestTools.UnitTesting;
using RabbitMQ.Client;
using Shouldly;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace RabbitFramework.Test
{
    [TestClass]
    public class RabbitBusProviderIntegrationTests
    {
        private const string Host = "localhost";
        private const int Port = 5672;
        private const string UserName = "guest";
        private const string Password = "guest";
        private const string ExchangeName = "testExchange";
        private const string Queue = "testQueue";
        private const string Topic = "testTopic";

        private RabbitBusProvider _sut;

        [TestInitialize]
        public void Initialize()
        {
            BusOptions busOptions = new BusOptions()
            {
                Hostname = Host,
                Port = Port,
                UserName = UserName,
                Password = Password,
                ExchangeName = ExchangeName
            };

            _sut = new RabbitBusProvider(busOptions);
        }

        [TestMethod]
        public void EventReceivedCallbackIsInvokedWithEvent()
        {
            string jsonMessage = "Something";

            EventMessage passedMessage = null;
            ManualResetEvent waitHandle = new ManualResetEvent(false);
            EventReceivedCallback eventReceivedCallback = (message) =>
            {
                passedMessage = message;
                waitHandle.Set();
            };

            _sut.CreateConnection();
            _sut.CreateQueueWithTopics(Queue, new List<string> { Topic });
            _sut.BasicConsume(Queue, eventReceivedCallback);

            SendRabbitEvent(jsonMessage);

            bool IsCalled = waitHandle.WaitOne(2000);

            IsCalled.ShouldBeTrue();
            passedMessage.ShouldNotBeNull();
            passedMessage.JsonMessage.ShouldBe(jsonMessage);
        }

        public void SendRabbitEvent(string json)
        {
            var factory = new ConnectionFactory()
            {
                HostName = Host,
                Port = Port,
                UserName = UserName,
                Password = Password
            };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.ExchangeDeclare(ExchangeName, "topic");
                channel.BasicPublish(exchange: ExchangeName,
                                     routingKey: Topic,
                                     basicProperties: null,
                                     body: Encoding.UTF8.GetBytes(json));
            }
        }
    }
}