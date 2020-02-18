
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;

namespace RxSockets
{
    public static class ServiceCollectionHostedServiceExtensions
    {
        public static IServiceCollection AsSelf(this IServiceCollection services)
        {
            ServiceDescriptor descriptor = services.LastOrDefault();
            if (descriptor != null)
            {
                Type implementationType = descriptor.GetImplementationType();
                if (descriptor.ServiceType == implementationType)
                {
                    return services;
                }
                if (descriptor.ImplementationInstance != null)
                {
                    services.Add(new ServiceDescriptor(implementationType, descriptor.ImplementationInstance));
                }
                else
                {
                    services.Remove(descriptor);
                    if (descriptor.ImplementationFactory != null)
                    {
                        services.Add(new ServiceDescriptor(descriptor.ImplementationType, descriptor.ImplementationFactory, descriptor.Lifetime));
                    }
                    else
                    {
                        services.Add(new ServiceDescriptor(descriptor.ImplementationType, descriptor.ImplementationType, descriptor.Lifetime));
                    }
                    services.Add(new ServiceDescriptor(descriptor.ServiceType, implementationType, descriptor.Lifetime));
                }
            }
            return services;
        }

        private static Type GetImplementationType(this ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType != null)
            {
                return descriptor.ImplementationType;
            }
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance.GetType();
            }
            return descriptor.ImplementationFactory != null ? descriptor.ImplementationFactory.GetType().GenericTypeArguments[1] : null;
        }
    }
}
