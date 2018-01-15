using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;
using Remiworks.Attributes.Models;

namespace Remiworks.Attributes.Initialization
{
    public static class InitializationUtility
    {
        public static Type GetParameterTypeOrThrow(MethodBase method, ILogger logger)
        {
            var parameterType = method
                .GetParameters()
                .FirstOrDefault()?
                .ParameterType;

            if (parameterType == null)
            {
                var exception = new InvalidOperationException(
                    $"No parameters could be found for method '{method.Name}' in type '{method.DeclaringType}'");

                logger.LogError(exception, "No parameters could be found for method {0} in type {1}", method.Name, method.DeclaringType);

                throw exception;
            }

            return parameterType;
        }

        public static IEnumerable<AttributeContent> GetAttributeValuesWithMethod<TAttribute>(Type type) 
            where TAttribute : ListenerAttribute
        {
            return type.GetMethods()
                .Where(m => m.GetCustomAttribute<TAttribute>() != null)
                .Select(AttributeContentSelector<TAttribute>)
                .ToList();
        }

        private static AttributeContent AttributeContentSelector<TAttribute>(MethodInfo method)
            where TAttribute : ListenerAttribute
        {
            var attribute = method.GetCustomAttribute<TAttribute>();

            return new AttributeContent
            {
                Method = method,
                ExchangeName = attribute.ExchangeName,
                Key = attribute.Topic
            };
        }
    }
}