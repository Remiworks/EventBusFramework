using System.Threading;
using Remiworks.Core.Event;
using Remiworks.Core.Event.Listener;

namespace Remiworks.Core.Test.Event.Stubs
{
    public class ListenToQueueAndTopicStub<T>
    {
        public EventWaitHandle WaitHandle { get; }
        public T ReceivedObject { get; private set; }

        public ListenToQueueAndTopicStub()
        {
            WaitHandle = new ManualResetEvent(false);
        }

        public EventReceivedForTopic<T> GenericTypeEventListenerCallback()
        {
            return (receivedObject) =>
            {
                ReceivedObject = receivedObject;
                WaitHandle.Set();
            };
        }

        public EventReceivedForTopic EventListenerCallback()
        {
            return (receivedObject) =>
            {
                ReceivedObject = (T) receivedObject;
                WaitHandle.Set();
            };
        }
    }
}