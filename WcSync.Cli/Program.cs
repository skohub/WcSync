using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WcSync.Db;
using WcSync.Wc;

namespace WcSync.Cli
{
    class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables("WcSync")
                .Build();

            new ProductService(new WcProductService(configuration), new DbProductRepository(configuration)).UpdateRecentProducts();
        }
    }
}
