using System;

namespace AttributeLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string CommandType { get; }

        public CommandAttribute(string commandType)
        {
            CommandType = commandType;
        }
    }
}