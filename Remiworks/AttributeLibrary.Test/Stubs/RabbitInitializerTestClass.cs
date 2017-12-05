using System;
using Remiworks.Attributes;

namespace RabbitFramework.Test
{
    [QueueListener("testQueue")]
    public class RabbitInitializerTestClass
    {
        [Event("testTopic")]
        public void TestModelTestFunction(TestModel message)
        {
            // Do stuff
        }

        [Event("testTwoTopic")]
        public void TestModelTestTwoFunctionThrowsArgumentException(TestModel message)
        {
            throw new ArgumentException();
        }
    }

    [QueueListener("testQueue")]
    public class RabbitInitializerCommandTestclass
    {
        [Command("testCommand")]
        public void TestModelTestFunction(TestModel message)
        {
            // Do stuff
        }

        [Command("testTwoCommand")]
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