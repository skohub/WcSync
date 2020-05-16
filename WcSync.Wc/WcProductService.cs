using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using ProductMeta = WooCommerceNET.WooCommerce.v2.ProductMeta;

namespace WcSync.Wc
{
    public class WcProductService : IWcProductService
    {
        private const string metaKey = "product_availability";

        private readonly IConfiguration _configuration;
        private readonly ILogger<WcProductService> _logger;
        private WCObject _wcObject;

        private WCObject WcClient => _wcObject ?? (_wcObject = Connect());

        public WcProductService(IConfiguration configuration, ILogger<WcProductService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task UpdateStockStatus(string sku, string stockStatus, IList<string> availability)
        {
            var product = await GetProductBySku(sku);

            var productId = product?.id ?? throw new Exception($"Product {sku} was not found in woocommerce");

            await WcClient.Product.Update(productId, new Product { 
                stock_status = stockStatus,
                meta_data = new List<ProductMeta>
                {
                    new ProductMeta 
                    {
                        key = metaKey,
                        value = string.Join(",", availability),
                    }
                }
            });

            return;
        }

        private async Task<Product> GetProductBySku(string sku)
        {
            var products = await WcClient.Product.GetAll(new Dictionary<string, string>
            { 
                { "sku", sku },
            });

            return products.FirstOrDefault() ?? throw new Exception($"Product with sku {sku} was not found");
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
