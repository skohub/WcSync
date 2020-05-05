using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WooCommerceNET.WooCommerce.v3.Extension;

namespace WcSync.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .AddEnvironmentVariables("WcSync")
                .Build();

            RestAPI rest = new RestAPI(
                $"{configuration["WcHost"]}/wp-json/wc/v3/", 
                configuration["WcClient"], 
                configuration["WcSecret"]);
            WCObject wc = new WCObject(rest);

            //Get all products
            var products = await wc.Product.GetAll();
            products.ForEach(p => Console.WriteLine(p.name));
        }
    }
}
