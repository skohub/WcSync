using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WcSync.Db;
using WcSync.Wc;

namespace WcSync.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serviceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();

            var productService = serviceProvider.GetService<IProductService>();
            await productService.UpdateRecentProductsAsync();
        }

        private static IServiceCollection ConfigureServices(IServiceCollection serviceCollection) {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables("WcSync")
                .Build();

            serviceCollection
                .AddSingleton<IWcProductService, WcProductService>()
                .AddSingleton<IDbProductRepository, DbProductRepository>()
                .AddSingleton<IProductService, ProductService>()
                .AddSingleton<IConfiguration>(configuration);

            serviceCollection.AddLogging(configure => configure.AddConsole(c => c.TimestampFormat = "[yyyy-MM-dd HH:mm:ss] "));

            return serviceCollection;
        }
    }
}
