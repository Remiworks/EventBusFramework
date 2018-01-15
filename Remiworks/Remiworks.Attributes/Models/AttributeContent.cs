using System.Reflection;

namespace Remiworks.Attributes.Models
{
    public class AttributeContent
    {
        public MethodInfo Method { get; set; }
        public string Key { get; set; }
        public string ExchangeName { get; set; }
    }
}