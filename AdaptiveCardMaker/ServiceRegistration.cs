using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;

namespace AdaptiveCardMaker
{
    public static class ServiceRegistration
    {
        public static IServiceCollection AddAdaptiveCardGenerator(this IServiceCollection services)
        {
            services.Configure<CardGeneratorOptions>(options =>
            {
                options.ProjectNamespace = options.AssemblyWithEmbeddedResources?.GetName()?.Name;
            });
            services.AddSingleton<ICardGenerator, CardGenerator>();
            return services;
        }
        public static IServiceCollection AddAdaptiveCardGenerator(this IServiceCollection services, Action<CardGeneratorOptions> options = null)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            services.Configure(options);
            services.AddSingleton<ICardGenerator, CardGenerator>();
            return services;
        }
        public static IServiceCollection AddAdaptiveCardGenerator<T>(this IServiceCollection services)
        {
            services.Configure<CardGeneratorOptions<T>>(options =>
            {
                options.AssemblyWithEmbeddedResources = Assembly.GetAssembly(typeof(T));
                options.ProjectNamespace = options.AssemblyWithEmbeddedResources?.GetName()?.Name;
            });
            services.AddSingleton<ICardGenerator<T>, CardGenerator<T>>();
            return services;
        }
        public static IServiceCollection AddAdaptiveCardGenerator<T>(this IServiceCollection services, Action<CardGeneratorOptions<T>> options = null)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            services.Configure(options);
            services.AddSingleton<ICardGenerator<T>, CardGenerator<T>>();
            return services;
        }
    }
}
