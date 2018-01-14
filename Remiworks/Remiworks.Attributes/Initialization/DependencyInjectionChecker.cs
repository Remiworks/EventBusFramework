using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace Remiworks.Attributes.Initialization
{
    public class DependencyInjectionChecker
    {
        public List<string> CheckDependencies(IServiceProvider serviceProvider, List<Type> types)
        {
            var errors = new List<string>();

            foreach (var type in types)
            {
                var constructors = type.GetConstructors();

                if (constructors.Length > 0)
                {
                    try
                    {
                        var instance = ActivatorUtilities.CreateInstance(serviceProvider, type);
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
