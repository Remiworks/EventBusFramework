using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Logging;

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

        public static Dictionary<string, MethodInfo> GetAttributeValuesWithMethod<TAttribute>(
            Type type,
            Func<TAttribute, string> predicate) where TAttribute : Attribute
        {
            return type.GetMethods()
                .Where(m => m.GetCustomAttribute<TAttribute>() != null)
                .ToDictionary(
                    m => predicate(m.GetCustomAttribute<TAttribute>()),
                    m => m);
        }
    }
}