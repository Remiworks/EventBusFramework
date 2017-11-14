using AttributeLibrary;
using RabbitFramework.Test;
using System;
using System.Collections.Generic;
using System.Text;

namespace TestConsoleAppRabbi
{
    [EventListener("user")]
    public class EventListenerTwo
    {
        [Topic("event.updated")]
        public void TestTopic(TestModel testModel)
        {
            Console.WriteLine("Updated: " + testModel.Name);
        }

        [Topic("event.selected")]
        public void TestTopicTwo(TestModel testModel)
        {
            Console.WriteLine("Selected: " + testModel.Name);
        }

        [Topic("event.deleted")]
        public void TestTopicThree(TestModel testModel)
        {
            Console.WriteLine("deleted2: " + testModel.Name);
        }
    }
}
