using System;

namespace AttributeLibrary.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class CommandAttribute : Attribute
    {
        public string Key { get; }

        public CommandAttribute(string commandType)
        {
            Key = commandType;
        }
    }
}