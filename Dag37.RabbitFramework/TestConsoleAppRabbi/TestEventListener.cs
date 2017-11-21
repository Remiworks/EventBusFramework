using AttributeLibrary;
using RabbitFramework.Test;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestConsoleAppRabbi
{
    [EventListener("user")]
    public class TestEventListener
    {
        [Topic("event.created")]
        public void TestTopic(TestModel testModel)
        {
            Console.WriteLine(testModel.Name);
        }

        [Topic("event.deleted")]
        public void TestTopicTwo(TestModel testModel)
        {
            Console.WriteLine("eventtwo" + testModel.Name);
        }
    }
}
