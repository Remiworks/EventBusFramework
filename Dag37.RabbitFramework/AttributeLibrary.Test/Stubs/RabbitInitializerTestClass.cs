using AttributeLibrary;
using AttributeLibrary.Attributes;
using System;

namespace RabbitFramework.Test
{
    [QueueListener("testQueue")]
    public class RabbitInitializerTestClass
    {
        [Topic("testTopic")]
        public void TestModelTestFunction(TestModel message)
        {
            // Do stuff
        }

        [Topic("testTwoTopic")]
        public void TestModelTestTwoFunctionThrowsArgumentException(TestModel message)
        {
            throw new ArgumentException();
        }
    }

    public class TestModel
    {
        public string Name { get; set; }
    }
}