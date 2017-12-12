using System;

namespace Remiworks.Attributes.Test.Stubs
{
    [QueueListener("testQueue1")]
    public class ClassWithEventsStub1
    {
        [Event("testTopic1")]
        public void TestTopic1(Person message)
        {
            // Do stuff
        }

        [Event("testTopic2")]
        public void TestTopic2WithException(Person message)
        {
            throw new ArgumentException();
        }
    }
}