using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Core.Command.Publisher;
using Shouldly;

namespace Remiworks.Core.Test.Command.Publisher
{
    [TestClass]
    public class CommandPublisherParameterTests
    {
        private const string WildcardExceptionMessage = "Key shouldn't contain wildcards";
        private const string QueueNameParamName = "queueName";
        private const string MessageParamName = "message";
        private const string KeyParamName = "key";
        
        private const string QueueName = "someQueue";
        private const string Key = "testKey";
        private const int Timeout = 0;
        
        private CommandPublisher _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new CommandPublisher(new Mock<IBusProvider>().Object);
        }
        
        [TestMethod]
        public void SendCommandThrows_ArgumentNullException_WhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => 
                _sut.SendCommandAsync<string>(new object(), null, Key, Timeout));
            
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() => 
                _sut.SendCommandAsync<string>(new object(), "", Key, Timeout));
            
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenQueueNameIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() => 
                _sut.SendCommandAsync<string>(new object(), " ", Key, Timeout));
            
            exception.ParamName.ShouldBe(QueueNameParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentNullException_WhenMessageIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => 
                _sut.SendCommandAsync<string>(null, QueueName, Key, Timeout));
            
            exception.ParamName.ShouldBe(MessageParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentNullException_WhenKeyIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() => 
                _sut.SendCommandAsync<string>(new object(), QueueName, null, Timeout));
            
            exception.ParamName.ShouldBe(KeyParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenKeyIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() => 
                _sut.SendCommandAsync<string>(new object(), QueueName, "", Timeout));
            
            exception.ParamName.ShouldBe(KeyParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenKeyIsWhitespace()
        {
            var exception = Should.Throw<ArgumentException>(() => 
                _sut.SendCommandAsync<string>(new object(), QueueName, " ", Timeout));
            
            exception.ParamName.ShouldBe(KeyParamName);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenKeyContainsWildCardStar()
        {
            var exception = Should.Throw<ArgumentException>(() => 
                _sut.SendCommandAsync<string>(new object(), QueueName, "test.*.event", Timeout));
            
            exception.Message.ShouldBe(WildcardExceptionMessage);
        }

        [TestMethod]
        public void SendCommandThrows_ArgumentException_WhenKeyContainsWildCardHashtag()
        {
            var exception = Should.Throw<ArgumentException>(() => 
                _sut.SendCommandAsync<string>(new object(), QueueName, "test.#.event", Timeout));
            
            exception.Message.ShouldBe(WildcardExceptionMessage);
        }
    }
}