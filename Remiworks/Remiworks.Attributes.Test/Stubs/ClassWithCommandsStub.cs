using System;

namespace Remiworks.Attributes.Test.Stubs
{
    [QueueListener("testQueue3")]
    public class ClassWithCommandsStub
    {
        [Command("testCommand")]
        public void TestModelTestFunction(Person message)
        {
            // Do stuff
        }

        [Command("testTwoCommand")]
        public void TestModelTestTwoFunctionThrowsArgumentException(Person message)
        {
            throw new ArgumentException();
        }
    }
}