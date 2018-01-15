using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;

namespace Remiworks.Attributes.Initialization
{
    public static class DependencyInjectionChecker
    {
        public static IEnumerable<string> CheckDependencies(IServiceProvider serviceProvider, List<Type> types)
        {
            var errors = new List<string>();

            foreach (var type in types)
            {
                var constructors = type.GetConstructors();

                if (constructors.Length > 0)
                {
                    try
                    {
                        ActivatorUtilities.CreateInstance(serviceProvider, type);
                    }
                    catch (Exception ex)
                    {
                        errors.Add(ex.Message);
                    }
                }
            }

            return errors;
        }
    }
}