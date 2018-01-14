using System;
using System.Runtime.Serialization;

namespace Remiworks.Attributes.Initialization
{
    [Serializable]
    internal class DependencyInjectionException : Exception
    {
        public DependencyInjectionException()
        {
        }

        public DependencyInjectionException(string message) : base(message)
        {
        }

        public DependencyInjectionException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected DependencyInjectionException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}