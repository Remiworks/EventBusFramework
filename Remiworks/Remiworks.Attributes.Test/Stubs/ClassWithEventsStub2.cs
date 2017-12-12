using System;

namespace Remiworks.Attributes.Test.Stubs
{
    [QueueListener("testQueue2")]
    public class ClassWithEventsStub2
    {
        [Event("testTopic3")]
        public void TestTopic3(Person message)
        {
            // Do stuff
        }

        [Event("testTopic4")]
        public void TestTopic4WithException(Person message)
        {
            throw new ArgumentException();
        }
    }
}