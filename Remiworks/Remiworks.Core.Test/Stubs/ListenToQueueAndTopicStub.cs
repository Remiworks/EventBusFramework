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

        public EventReceived<T> GenericTypeEventListenerCallback()
        {
            return (receivedObject, topic) =>
            {
                ReceivedObject = receivedObject;
                WaitHandle.Set();
            };
        }

        public EventReceived EventListenerCallback()
        {
            return (receivedObject, topic) =>
            {
                ReceivedObject = (T) receivedObject;
                WaitHandle.Set();
            };
        }
    }
}