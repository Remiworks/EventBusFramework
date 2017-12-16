using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Remiworks.Core.Command;
using Remiworks.Core.Test.Stubs;
using Shouldly;

namespace Remiworks.Core.Test.Command
{
    [TestClass]
    public class CommandListenerParameterTests
    {
        private const string KeyContainsWildcardMessage = "Key should not contain wildcards";
        private const string QueueNameParameter = "queueName";
        private const string KeyParameter = "key";
        private const string CallbackParameter = "callback";
        private const string ParameterTypeParameter = "parameterType";
        
        private const string QueueName = "someQueue";
        private const string Key = "some.key";
        
        private CommandListener _sut;

        [TestInitialize]
        public void Initialize()
        {
            _sut = new CommandListener(new Mock<IBusProvider>().Object);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncGenericThrows_ArgumentNullException_WhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupCommandListenerAsync(null, Key, new Mock<CommandReceivedCallback<Person>>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncGenericThrows_ArgumentException_WhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync("", Key, new Mock<CommandReceivedCallback<Person>>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncGenericThrows_ArgumentException_WhenQueueNameIsWhiteSpace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(" ", Key, new Mock<CommandReceivedCallback<Person>>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncGenericThrows_ArgumentNullException_WhenKeyIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, null, new Mock<CommandReceivedCallback<Person>>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(KeyParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncGenericThrows_ArgumentException_WhenKeyIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, "", new Mock<CommandReceivedCallback<Person>>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(KeyParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncGenericThrows_ArgumentException_WhenKeyIsWhiteSpace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, " ", new Mock<CommandReceivedCallback<Person>>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(KeyParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncGenericThrows_ArgumentException_WhenKeyContainsStarWildcard()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, "some.*", new Mock<CommandReceivedCallback<Person>>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(KeyParameter);
            exception.Message.ShouldStartWith(KeyContainsWildcardMessage);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncGenericThrows_ArgumentException_WhenKeyContainsHashtagWildcard()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, "some.#", new Mock<CommandReceivedCallback<Person>>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(KeyParameter);
            exception.Message.ShouldStartWith(KeyContainsWildcardMessage);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncGenericThrows_ArgumentNullException_WhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync<Person>(QueueName, Key, null)
                    .Wait());
            
            exception.ParamName.ShouldBe(CallbackParameter);
        }
       
        [TestMethod]
        public void SetupCommandListenerAsyncThrows_ArgumentNullException_WhenQueueNameIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupCommandListenerAsync(null, Key, new Mock<CommandReceivedCallback>().Object, new Mock<Type>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncThrows_ArgumentException_WhenQueueNameIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync("", Key, new Mock<CommandReceivedCallback>().Object, new Mock<Type>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncThrows_ArgumentException_WhenQueueNameIsWhiteSpace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(" ", Key, new Mock<CommandReceivedCallback>().Object, new Mock<Type>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(QueueNameParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncThrows_ArgumentNullException_WhenKeyIsNull()
        {
            var exception = Should.Throw<ArgumentNullException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, null, new Mock<CommandReceivedCallback>().Object, new Mock<Type>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(KeyParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncThrows_ArgumentException_WhenKeyIsEmpty()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, "", new Mock<CommandReceivedCallback>().Object, new Mock<Type>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(KeyParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncThrows_ArgumentException_WhenKeyIsWhiteSpace()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, " ", new Mock<CommandReceivedCallback>().Object, new Mock<Type>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(KeyParameter);
        }
        

        [TestMethod]
        public void SetupCommandListenerAsyncThrows_ArgumentException_WhenKeyContainsStarWildcard()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, "some.*", new Mock<CommandReceivedCallback>().Object, new Mock<Type>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(KeyParameter);
            exception.Message.ShouldStartWith(KeyContainsWildcardMessage);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncThrows_ArgumentException_WhenKeyContainsHashtagWildcard()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, "some.#", new Mock<CommandReceivedCallback>().Object, new Mock<Type>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(KeyParameter);
            exception.Message.ShouldStartWith(KeyContainsWildcardMessage);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncThrows_ArgumentNullException_WhenCallbackIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, Key, null, new Mock<Type>().Object)
                    .Wait());
            
            exception.ParamName.ShouldBe(CallbackParameter);
        }

        [TestMethod]
        public void SetupCommandListenerAsyncThrows_ArgumentNullException_WhenTypeIsNull()
        {
            var exception = Should.Throw<ArgumentException>(() =>
                _sut.SetupCommandListenerAsync(QueueName, Key, new Mock<CommandReceivedCallback>().Object, null)
                    .Wait());
            
            exception.ParamName.ShouldBe(ParameterTypeParameter);
        }
    }
}