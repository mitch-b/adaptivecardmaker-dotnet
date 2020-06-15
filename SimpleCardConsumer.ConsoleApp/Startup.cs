using AdaptiveCardMaker;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleCardConsumer.ConsoleApp
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
            services.AddTransient<Client>();
            return services;
        }
    }
}
