using AttributeLibrary;
using System;
using System.Collections.Generic;
using System.Text;

namespace RabbitFramework.Test
{
    [EventListener("testQueue")]
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
