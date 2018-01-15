using System;

namespace Remiworks.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : ListenerAttribute
    {
        public CommandAttribute(string commandType, string exchangeName = null) : base(commandType, exchangeName)
        {
        }
    }
}