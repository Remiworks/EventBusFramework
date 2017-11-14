using AttributeLibrary;
using RabbitFramework.Test;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestConsoleAppRabbi
{
    [EventListener("TestEvent")]
    public class TestEventListener
    {
        [Topic("test.event")]
        public void TestTopic(TestModel testModel)
        {
            Console.WriteLine(testModel.Name);
        }

        [Topic("test.eventtwo")]
        public void TestTopicTwo(TestModel testModel)
        {
            Console.WriteLine("eventtwo" + testModel.Name);
        }
    }
}
