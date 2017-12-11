using System.Threading;
using Remiworks.Core.Event;

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

        public EventReceivedForTopic<T> EventListenerCallback()
        {
            return (receivedObject) =>
            {
                ReceivedObject = receivedObject;
                WaitHandle.Set();
            };
        }
    }
}