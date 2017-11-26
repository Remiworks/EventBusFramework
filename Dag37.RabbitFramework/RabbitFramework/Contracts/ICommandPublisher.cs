using System.Threading.Tasks;

namespace RabbitFramework.Contracts
{
    public interface ICommandPublisher
    {
        Task<TResult> SendCommand<TResult>(object message, string queueName, string key, int timeout = 5000);
    }

    public delegate object CommandReceivedCallback<in TParam>(TParam command);
}