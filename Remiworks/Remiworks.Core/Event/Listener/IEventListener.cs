using System;
using System.Threading.Tasks;

namespace Remiworks.Core.Event.Listener
{
    public interface IEventListener
    {
        Task SetupQueueListenerAsync<TParam>(string queueName, string topic, EventReceived<TParam> callback, string exchangeName = null);

        Task SetupQueueListenerAsync(string queueName, string topic, EventReceived callback, Type parameterType, string exchangeName = null);
    }
    
    public delegate void EventReceived<in TParam>(TParam input, string topic);

    public delegate void EventReceived(object input, string topic);
}