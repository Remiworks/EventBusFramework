using AttributeLibrary;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Reflection;

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

            var executingAssembly = Assembly.GetCallingAssembly();

            var types = executingAssembly.GetTypes();
            SearchTypes(types);
        }

        private void SearchTypes(Type[] types)
        {
            foreach (var type in types)
            {
                var eventAttribute = type.GetCustomAttribute<EventListenerAttribute>();

                if (eventAttribute != null)
                {
                    RegisterQueue(eventAttribute.QueueName);
                    SearchMethodsInType(type, eventAttribute);
                }
            }
        }

        private void SearchMethodsInType(Type type, EventListenerAttribute eventAttribute)
        {
            foreach (var methodInfo in type.GetMethods())
            {
                var topicAttribute = methodInfo.GetCustomAttribute<TopicAttribute>();

                if (topicAttribute != null)
                {
                    RegisterTopic(eventAttribute.QueueName, topicAttribute.Topic, type, methodInfo);
                }
            }
        }

        public void RegisterQueue(string queue)
        {
            _busProvider.CreateQueue(queue);
        }

        public void RegisterTopic(string queue, string topic, Type type, MethodInfo methodInfo)
        {
            _busProvider.BasicConsume(queue, topic, CreateEventReceivedCallback(type, methodInfo));
        }

        public EventReceivedCallback CreateEventReceivedCallback(Type type, MethodInfo methodInfo)
        {
            return (message) =>
            {
                var instance = Activator.CreateInstance(type);

                var parameters = methodInfo.GetParameters();
                var parameter = parameters.FirstOrDefault();
                var paramType = parameter.ParameterType;
                var arguments = JsonConvert.DeserializeObject(message.JsonMessage, paramType);

                try
                {
                    methodInfo.Invoke(instance, new object[] { arguments });
                }
                catch (TargetInvocationException)
                {
                    throw;
                }
            };
        }
    }
}