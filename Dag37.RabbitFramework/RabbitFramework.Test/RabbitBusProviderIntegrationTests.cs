using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
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
        private const string TopicType = "topic";

        private readonly BusOptions BusOptions = new BusOptions()
        {
            Hostname = Host,
            Port = Port,
            UserName = UserName,
            Password = Password,
            ExchangeName = ExchangeName
        };

        private IConnection _connection;
        private IModel _channel;

        [TestInitialize]
        public void Initialize()
        {
            OpenRabbitConnection();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _connection.Dispose();
            _channel.Dispose();
        }

        [TestMethod]
        public void EventCanBeReceived()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                string queue = UniqueQueue();
                string topic = UniqueTopic();
                string jsonMessage = "Something";

                EventMessage passedMessage = null;
                ManualResetEvent waitHandle = new ManualResetEvent(false);
                EventReceivedCallback eventReceivedCallback = (message) =>
                {
                    passedMessage = message;
                    waitHandle.Set();
                };

                sut.CreateQueueWithTopics(queue, new List<string> { topic });
                sut.BasicConsume(queue, eventReceivedCallback);

                SendRabbitEventToExchange(topic, jsonMessage);

                waitHandle.WaitOne(2000).ShouldBeTrue();
                passedMessage.ShouldNotBeNull();
                passedMessage.JsonMessage.ShouldBe(jsonMessage);
            }
        }

        [TestMethod]
        public void EventCanBePublished()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                string queue = UniqueQueue();
                string topic = UniqueTopic();

                ManualResetEvent waitHandle = new ManualResetEvent(false);
                BasicDeliverEventArgs passedArgs = null;

                EventMessage message = new EventMessage
                {
                    JsonMessage = "Something",
                    RoutingKey = topic,
                    Type = TopicType
                };

                ConsumeRabbitEvent(queue, (sender, args) =>
                {
                    waitHandle.Set();
                    passedArgs = args;
                });

                sut.CreateQueueWithTopics(queue, new List<string> { topic });
                sut.BasicPublish(message);

                waitHandle.WaitOne(2000).ShouldBeTrue();
                string receivedMessage = Encoding.UTF8.GetString(passedArgs.Body);
                receivedMessage.ShouldBe(message.JsonMessage);
                passedArgs.RoutingKey.ShouldBe(message.RoutingKey);
            }
        }

        [TestMethod]
        public void EventCanBePublishedAndReceived()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                string queue = UniqueQueue();
                string topic = UniqueTopic();

                EventMessage receivedMessage = null;
                ManualResetEvent waitHandle = new ManualResetEvent(false);
                EventReceivedCallback eventReceivedCallback = (message) =>
                {
                    receivedMessage = message;
                    waitHandle.Set();
                };

                EventMessage sentMessage = new EventMessage
                {
                    JsonMessage = "Something",
                    RoutingKey = topic,
                    Type = TopicType
                };

                sut.CreateQueueWithTopics(queue, new List<string> { topic });
                sut.BasicConsume(queue, eventReceivedCallback);
                sut.BasicPublish(sentMessage);

                waitHandle.WaitOne(2000).ShouldBeTrue();
                receivedMessage.JsonMessage.ShouldBe(sentMessage.JsonMessage);
            }
        }

        [TestMethod]
        public void CommandCanBeReceived()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                string queue = UniqueQueue();
                string correlationId = new Guid().ToString();

                CommandStub sentCommand = new CommandStub { Value = "SomeValue" };
                string sentCommandJson = JsonConvert.SerializeObject(sentCommand);

                CommandStub receivedCommand = null;
                ManualResetEvent waitHandle = new ManualResetEvent(false);
                CommandReceivedCallback<CommandStub> commandReceivedCallback = (command) =>
                {
                    receivedCommand = command;
                    waitHandle.Set();

                    return "Something";
                };

                sut.SetupRpcListener(queue, commandReceivedCallback);
                SendRabbitEventToQueue(queue, correlationId, sentCommandJson);

                waitHandle.WaitOne(2000).ShouldBeTrue();
                receivedCommand.Value.ShouldBe(sentCommand.Value);
            }
        }

        [TestMethod]
        public void CommandCanBeSent()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                string queue = UniqueQueue();
                CommandStub sentCommand = new CommandStub { Value = "SomeValue" };

                BasicDeliverEventArgs receivedEventArgs = null;
                ManualResetEvent waitHandle = new ManualResetEvent(false);
                EventHandler<BasicDeliverEventArgs> commandCallback = (sender, eventArgs) =>
                {
                    receivedEventArgs = eventArgs;
                    waitHandle.Set();
                };

                ConsumeRabbitEvent(queue, commandCallback);

                // Timeout exception is expected. We dont send a response to the calling party
                var exception = Should.Throw<AggregateException>(() => sut.Call<string>(queue, sentCommand, 0).Wait());
                exception.InnerException.ShouldBeOfType<TimeoutException>();

                waitHandle.WaitOne(2000).ShouldBeTrue();

                receivedEventArgs.BasicProperties.CorrelationId.ShouldNotBeNullOrEmpty();
                receivedEventArgs.BasicProperties.ReplyTo.ShouldNotBeNullOrEmpty();

                string commandJson = Encoding.UTF8.GetString(receivedEventArgs.Body);
                CommandStub receivedCommand = JsonConvert.DeserializeObject<CommandStub>(commandJson);
                receivedCommand.Value.ShouldBe(sentCommand.Value);
            }
        }

        [TestMethod]
        public void CommandCanBeSentAndReceived()
        {
            using (var sut = new RabbitBusProvider(BusOptions))
            {
                string queue = UniqueQueue();
                CommandStub sentCommand = new CommandStub { Value = "SomeValue" };
                
                sut.SetupRpcListener<CommandStub>(queue, (command) => command.Value.Reverse().ToString());
                string result = sut.Call<string>(queue, sentCommand).Result;

                result.ShouldBe(sentCommand.Value.Reverse().ToString());
            }
        }

        private string UniqueQueue()
        {
            return $"TestQueue-{Guid.NewGuid()}";
        }

        private string UniqueTopic()
        {
            return $"TestTopic.{Guid.NewGuid()}";
        }

        private void OpenRabbitConnection()
        {
            var factory = new ConnectionFactory()
            {
                HostName = Host,
                Port = Port,
                UserName = UserName,
                Password = Password
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.ExchangeDeclare(ExchangeName, TopicType);
        }

        private void SendRabbitEventToQueue(string queue, string correlationId, string json)
        {
            var properties = _channel.CreateBasicProperties();
            properties.CorrelationId = correlationId;

            _channel.BasicPublish(
                exchange: "",
                routingKey: queue,
                basicProperties: properties,
                body: Encoding.UTF8.GetBytes(json));
        }

        private void SendRabbitEventToExchange(string topic, string json)
        {
            _channel.BasicPublish(exchange: ExchangeName,
                                 routingKey: topic,
                                 basicProperties: null,
                                 body: Encoding.UTF8.GetBytes(json));
        }

        private void ConsumeRabbitEvent(string queue, EventHandler<BasicDeliverEventArgs> callback)
        {
            _channel.QueueDeclare(queue: queue, exclusive: false);

            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += callback;

            _channel.BasicConsume(queue, true, consumer);
        }
    }
}