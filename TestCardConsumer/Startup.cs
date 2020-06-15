using AdaptiveCardMaker;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using TestCardConsumer.CardLibrary;

namespace TestCardConsumer.ConsoleApp
{
    class Startup
    {
        static void Main(string[] args)
        {
            var services = ConfigureServices();
            var serviceProvider = services.BuildServiceProvider();

            // Kick off our actual code
            serviceProvider.GetService<Client>().Run();
        }

        private static IServiceCollection ConfigureServices()
        {
            IServiceCollection services = new ServiceCollection();
            services.AddAdaptiveCardGenerator();
            services.AddAdaptiveCardGenerator<ImportedCardLibrary>(options =>
            {
                options.AssemblyWithEmbeddedResources = Assembly.GetAssembly(typeof(ImportedCardLibrary));
                options.ProjectNamespace = options.AssemblyWithEmbeddedResources?.GetName()?.Name;
                options.ManifestResourcePathFromNamespace = "MyCards";
            });
            services.AddTransient<Client>();
            return services;
        }
    }
}
