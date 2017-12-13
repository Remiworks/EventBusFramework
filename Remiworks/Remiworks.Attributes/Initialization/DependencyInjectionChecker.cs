using Microsoft.Extensions.DependencyInjection;
using Remiworks.Core;
using Remiworks.Core.Event;
using System;
using System.Collections.Generic;
using System.Text;

namespace Remiworks.Attributes.Initialization
{
    public class DependencyInjectionChecker
    {
        public List<string> CheckDependencies(IServiceProvider serviceProvider, List<Type> types)
        {
            var errors = new List<string>();

            foreach (var type in types)
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

            return errors;
        }
    }
}
