﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using WcSync.Db;
using WcSync.Wc;

namespace WcSync.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = ConfigureServices(new ServiceCollection()).BuildServiceProvider();

            var productService = serviceProvider.GetService<IProductService>();
            productService.UpdateRecentProducts();
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

            return serviceCollection;
        }
    }
}
