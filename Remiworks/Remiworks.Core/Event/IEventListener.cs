using System.Threading.Tasks;

namespace Remiworks.Core.Event
{
    public interface IEventListener
    {
        Task SetupQueueListener<TParam>(string queueName, EventReceived<TParam> callback);

        Task SetupQueueListener<TParam>(string queueName, string topic, EventReceivedForTopic<TParam> callback);
    }
    
    public delegate void EventReceived<in TParam>(TParam input, string topic);

    public delegate void EventReceivedForTopic<in TParam>(TParam input);
}