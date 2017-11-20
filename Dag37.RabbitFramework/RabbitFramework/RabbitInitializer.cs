using AttributeLibrary;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
                    Dictionary<string, MethodInfo> topicsWithMethods = GetTopicsWithMethods(type);
                    _busProvider.CreateQueueWithTopics(eventAttribute.QueueName, topicsWithMethods.Keys);
                    var callback = CreateEventReceivedCallback(type, topicsWithMethods);
                    _busProvider.BasicConsume(eventAttribute.QueueName, callback);
                }
            }
        }

        public Dictionary<string, MethodInfo> GetTopicsWithMethods(Type type)
        {
            var topicsWithMethods = new Dictionary<string, MethodInfo>();
            foreach (var methodInfo in type.GetMethods())
            {
                var topicAttribute = methodInfo.GetCustomAttribute<TopicAttribute>();

                if (topicAttribute != null)
                {
                    topicsWithMethods.Add(topicAttribute.Topic, methodInfo);
                }
            }

            return topicsWithMethods;
        }

        public EventReceivedCallback CreateEventReceivedCallback(Type type, Dictionary<string, MethodInfo> topics)
        {
            return (message) =>
            {
                var instance = Activator.CreateInstance(type);

                var topicMatches = GetTopicMatches(message.RoutingKey, topics);

                foreach (var topic in topicMatches)
                {
                    try
                    {
                        var parameters = topic.Value.GetParameters();
                        var parameter = parameters.FirstOrDefault();
                        var paramType = parameter.ParameterType;
                        var arguments = JsonConvert.DeserializeObject(message.JsonMessage, paramType);

                        topic.Value.Invoke(instance, new object[] { arguments });
                    }
                    catch (TargetInvocationException)
                    {
                        throw;
                    }
                }
            };
        }

        public Dictionary<string, MethodInfo> GetTopicMatches(string routingKey, Dictionary<string, MethodInfo> topics)
        {
            var regexHashTag = @"\w+(\.\w+)*";
            var regexStar = @"[\w]+";
            var topicMatches = new Dictionary<string, MethodInfo>();

            foreach (var topic in topics)
            {
                var pattern = topic.Key
                    .Replace(".", "\\.")
                    .Replace("*", regexStar)
                    .Replace("#", regexHashTag);

                pattern = $"^{pattern}$";

                if (Regex.IsMatch(routingKey, pattern))
                {
                    topicMatches.Add(topic.Key, topic.Value);
                }
            }

            return topicMatches;
        }
    }
}