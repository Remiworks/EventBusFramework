using System;
using System.Threading.Tasks;

namespace Remiworks.Core.Event.Listener
{
    public interface IEventListener
    {
        Task SetupQueueListenerAsync<TParam>(string queueName, EventReceived<TParam> callback);

        Task SetupQueueListenerAsync(string queueName, EventReceived callback, Type parameterType);
        
        Task SetupQueueListenerAsync<TParam>(string queueName, string topic, EventReceivedForTopic<TParam> callback);

        Task SetupQueueListenerAsync(string queueName, string topic, EventReceivedForTopic callback, Type parameterType);
    }
    
    public delegate void EventReceived<in TParam>(TParam input, string topic);

    public delegate void EventReceivedForTopic<in TParam>(TParam input);

    public delegate void EventReceived(object input, string topic);

    public delegate void EventReceivedForTopic(object input);
}