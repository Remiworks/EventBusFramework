using System;
using System.Reflection;
using EnsureThat;
using Remiworks.Attributes.Initialization;

namespace Remiworks.Attributes.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void UseAttributes(this IServiceProvider serviceProvider)
        {
            EnsureArg.IsNotNull(serviceProvider, nameof(serviceProvider));

            var initializer = new Initializer(serviceProvider);
            initializer.Initialize(Assembly.GetCallingAssembly());
        }
    }
}