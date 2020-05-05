using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using WooCommerceNET.WooCommerce.v3.Extension;

namespace WcSync.Wc
{
    public class WcProductService : IWcProductService
    {
        private readonly IConfiguration _configuration;
        private WCObject _wcObject;

        private WCObject WcClient => _wcObject ?? (_wcObject = Connect());

        public WcProductService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<int> GetProductIdBySku(string sku)
        {
            var products = await WcClient.Product.GetAll(new Dictionary<string, string>
            { 
                { "sku", sku },
            });

            return products.FirstOrDefault().id ?? throw new Exception($"Product {sku} id was not found");
        }

        public async Task SetStockStatus(int id, string stockStatus)
        {
            await WcClient.Product.Update(id, new Product { stock_status = stockStatus });

            return;
        }

        private WCObject Connect() 
        {
            RestAPI rest = new RestAPI(
                $"{_configuration["WcHost"]}/wp-json/wc/v3/", 
                _configuration["WcClient"], 
                _configuration["WcSecret"]);

            return new WCObject(rest);
        }
    }
}
