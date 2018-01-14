using System;
using System.Threading.Tasks;
using EnsureThat;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Remiworks.Core.Event.Listener.Callbacks;
using Remiworks.Core.Models;

namespace Remiworks.Core.Event.Listener
{
    public class EventListener : IEventListener
    {
        private ILogger Logger { get; } = RemiLogging.CreateLogger<EventListener>();

        private readonly IBusProvider _busProvider;
        private readonly IEventCallbackRegistry _callbackRegistry;

        public EventListener(IBusProvider busProvider, IEventCallbackRegistry callbackRegistry)
        {
            _busProvider = busProvider;
            _callbackRegistry = callbackRegistry;

            _busProvider.EnsureConnection();
        }

        public Task SetupQueueListenerAsync<TParam>(
            string queueName,
            string topic,
            EventReceived<TParam> callback,
            string exchangeName = null)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNullOrWhiteSpace(topic, nameof(topic));
            EnsureArg.IsNotNull(callback, nameof(callback));

            return SetupQueueListenerAsync(
                queueName,
                topic,
                (input, receivedTopic) => callback((TParam)input, receivedTopic),
                typeof(TParam),
                exchangeName);
        }

        public Task SetupQueueListenerAsync(
            string queueName,
            string topic,
            EventReceived callback,
            Type parameterType,
            string exchangeName = null)
        {
            EnsureArg.IsNotNullOrWhiteSpace(queueName, nameof(queueName));
            EnsureArg.IsNotNullOrWhiteSpace(topic, nameof(topic));
            EnsureArg.IsNotNull(callback, nameof(callback));
            EnsureArg.IsNotNull(parameterType, nameof(parameterType));

            LogSetupQueueListenerCalled(queueName, topic, parameterType, exchangeName);

            return Task.Run(() =>
            {
                void CallbackInvoker(EventMessage eventMessage)
                {
                    LogReceivedCallback(queueName, topic, eventMessage.RoutingKey, eventMessage.JsonMessage);

                    object deserializedParameter;

                    try
                    {
                        deserializedParameter = JsonConvert.DeserializeObject(eventMessage.JsonMessage, parameterType);
                    }
                    catch (Exception exception)
                    {
                        LogFailedJsonConvert(queueName, topic, parameterType, eventMessage.JsonMessage, exception);
                        throw;
                    }

                    try
                    {
                        LogCallingCallback(queueName, topic);
                        callback(deserializedParameter, eventMessage.RoutingKey);
                        LogDoneCallingCallback(queueName, topic);
                    }
                    catch (Exception exeption)
                    {
                        LogFailedCallingCallback(queueName, topic, exeption);
                        throw;
                    }
                }

                _callbackRegistry.AddCallbackForQueue(queueName, topic, CallbackInvoker, exchangeName);

                LogSetupQueueListenerDone(queueName, topic, parameterType, exchangeName);
            });
        }

        private void LogSetupQueueListenerCalled(string queueName, string topic, Type parameterType, string exchangeName)
        {
            if (exchangeName == null)
            {
                Logger.LogInformation(
                    "Setting up queue listener for queue {0}, topic {1} and parameter type {2}",
                    queueName,
                    topic,
                    parameterType.FullName);
            }
            else
            {
                Logger.LogInformation(
                    "Setting up queue listener for queue {0}, topic {1}, parameter type {2} and exchange {3}",
                    queueName,
                    topic,
                    parameterType.FullName,
                    exchangeName);
            }
        }

        private void LogSetupQueueListenerDone(string queueName, string topic, Type parameterType, string exchangeName)
        {
            if (exchangeName == null)
            {
                Logger.LogInformation(
                    "Done setting up queue listener for queue {0}, topic {1} and parameter type {2}",
                    queueName,
                    topic,
                    parameterType.FullName);
            }
            else
            {
                Logger.LogInformation(
                    "Done setting up queue listener for queue {0}, topic {1}, parameter type {2} and exchange {3}",
                    queueName,
                    topic,
                    parameterType.FullName,
                    exchangeName);
            }
        }

        private void LogReceivedCallback(string queueName, string partialTopic, string fullTopic, string jsonMessage)
        {
            Logger.LogInformation(
                "Received callback for queue queue {0} and topic {1}. Full received topic: {2}. Json: {3}",
                queueName,
                partialTopic,
                fullTopic,
                jsonMessage);
        }

        private void LogFailedJsonConvert(string queueName, string topic, Type parameterType, string json, Exception exception)
        {
            Logger.LogError(
                exception,
                "Failed converting to type {0} for queue {1} and topic {2}. Json: {3}",
                parameterType.FullName,
                queueName,
                topic,
                json);
        }

        private void LogCallingCallback(string queueName, string topic)
        {
            Logger.LogInformation(
                "Calling callback for queue queue {0} and topic {1}",
                queueName,
                topic);
        }

        private void LogDoneCallingCallback(string queueName, string topic)
        {
            Logger.LogInformation(
                "Done calling callback for queue queue {0} and topic {1}",
                queueName,
                topic);
        }

        private void LogFailedCallingCallback(string queueName, string topic, Exception exception)
        {
            Logger.LogError(
                exception,
                "Failed calling callback for queue queue {0} and topic {1}",
                queueName,
                topic);
        }
    }
}