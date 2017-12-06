using System;
using System.Reflection;
using Remiworks.Attributes.Initialization;
using Remiworks.Core;

namespace Remiworks.Attributes.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static void UseAttributes(this IServiceProvider serviceProvider)
        {
            var busProvider = serviceProvider.GetService(typeof(IBusProvider));

            if (busProvider == null)
            {
                throw new NotImplementedException("You did not use the AddRabbitMq method on the service collection");
            }

            var initializer = new Initializer((IBusProvider)busProvider, serviceProvider);

            initializer.Initialize(Assembly.GetCallingAssembly());
        }
    }
}