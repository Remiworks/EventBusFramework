using System;
using System.Text;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using RabbitFramework.Contracts;
using RabbitFramework.IntegrationTest.Stubs;
using RabbitFramework.Publishers;
using RabbitMQ.Client.Events;
using Shouldly;

namespace RabbitFramework.IntegrationTest.Publishers
{
    [TestClass]
    public class CommandPublisherIntegrationTests : RabbitIntegrationTest
    {
        private const string Key = "testKey";

        private IBusProvider _busProvider;

        private CommandPublisher _sut;

        [TestInitialize]
        public void Initialize()
        {
            _busProvider = new RabbitBusProvider(BusOptions);
            _busProvider.CreateConnection();

            _sut = new CommandPublisher(_busProvider);
        }

        [TestCleanup]
        public void Cleanup()
        {
            _busProvider.Dispose();
        }

        [TestMethod]
        public void CommandCanBeSent()
        {
            string queue = GetUniqueQueue();
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
            var exception = Should.Throw<AggregateException>(() => _sut.SendCommand<string>(sentCommand, queue, Key).Wait());
            exception.InnerException.ShouldBeOfType<TimeoutException>();

            waitHandle.WaitOne(2000).ShouldBeTrue();

            Thread.Sleep(5000);

            receivedEventArgs.BasicProperties.CorrelationId.ShouldNotBeNullOrEmpty();
            receivedEventArgs.BasicProperties.ReplyTo.ShouldNotBeNullOrEmpty();

            string commandJson = Encoding.UTF8.GetString(receivedEventArgs.Body);
            CommandStub receivedCommand = JsonConvert.DeserializeObject<CommandStub>(commandJson);
            receivedCommand.Value.ShouldBe(sentCommand.Value);
        }

        //[TestMethod]
        //public void CommandCanBeReceived()
        //{
        //    using (var sut = new RabbitBusProvider(BusOptions))
        //    {
        //        string queue = GetUniqueQueue();
        //        string correlationId = new Guid().ToString();

        //        CommandStub sentCommand = new CommandStub { Value = "SomeValue" };
        //        string sentCommandJson = JsonConvert.SerializeObject(sentCommand);

        //        CommandStub receivedCommand = null;
        //        ManualResetEvent waitHandle = new ManualResetEvent(false);
        //        CommandReceivedCallback<CommandStub> commandReceivedCallback = (command) =>
        //        {
        //            receivedCommand = command;
        //            waitHandle.Set();

        //            return "Something";
        //        };

        //        sut.CreateConnection();
        //        sut.SetupRpcListener(queue, commandReceivedCallback);
        //        SendRabbitEventToQueue(queue, correlationId, sentCommandJson);

        //        waitHandle.WaitOne(2000).ShouldBeTrue();
        //        receivedCommand.Value.ShouldBe(sentCommand.Value);
        //    }
        //}

        //[TestMethod]
        //public void CommandCanBeSentAndReceived()
        //{
        //    using (var sut = new RabbitBusProvider(BusOptions))
        //    {
        //        string queue = GetUniqueQueue();
        //        CommandStub sentCommand = new CommandStub { Value = "SomeValue" };

        //        sut.CreateConnection();

        //        sut.SetupRpcListener<CommandStub>(queue, (command) => command.Value.Reverse().ToString());
        //        string result = sut.Call<string>(queue, sentCommand).Result;

        //        result.ShouldBe(sentCommand.Value.Reverse().ToString());
        //    }
        //}
    }
}