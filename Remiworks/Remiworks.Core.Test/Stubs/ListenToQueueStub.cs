using System.Threading;
using Remiworks.Core.Event;

namespace Remiworks.Core.Test.Event.Stubs
{
    public class ListenToQueueStub<T>
    {
        public EventWaitHandle WaitHandle { get; }
        public string ReceivedTopic { get; private set; }
        public T ReceivedObject { get; private set; }

        public ListenToQueueStub()
        {
            WaitHandle = new ManualResetEvent(false);
        }
        
        public EventReceived<T> GenericTypeEventListenerCallback()
        {
            return (receivedObject, topic) =>
            {
                ReceivedObject = receivedObject;
                ReceivedTopic = topic;
                WaitHandle.Set();
            };
        }

        public EventReceived EventListenerCallback()
        {
            return (receivedObject, topic) =>
            {
                ReceivedObject = (T) receivedObject;
                ReceivedTopic = topic;
                WaitHandle.Set();
            };
        }
    }
}