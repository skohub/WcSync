using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WooCommerceNET;
using WooCommerceNET.WooCommerce.v3;
using Product = WooCommerceNET.WooCommerce.v3.Product;
using ProductMeta = WooCommerceNET.WooCommerce.v2.ProductMeta;
using WcSync.Model.Entities;
using System.Net;
using WcSync.Model;

namespace WcSync.Wc
{
    public class WcProductService : IWcProductService
    {
        private const string _metaKey = "product_availability";
        private const char _separator = ',';
        private const string _productsPerPage = "100";

        private readonly IConfiguration _configuration;
        private readonly ILogger<WcProductService> _logger;
        private WCObject _wcObject;
        private int _totalPages;

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
                        key = _metaKey,
                        value = string.Join(_separator.ToString(), availability),
                    }
                }
            });

            return;
        }

        public async Task<List<Model.Entities.Product>> GetProductsAsync()
        {
            var products = new List<Product>();
            var page = 1;
            _totalPages = 1; // Until we figure out by parsing response headers

            // Keep requests single threaded to prevent WooCommerce server overload
            while (page <= _totalPages)
            {
                _logger.LogInformation($"Retrieveing products, page {page}/{_totalPages}");

                try
                {
                    products.AddRange(await WcClient.Product.GetAll(new Dictionary<string, string> 
                    { 
                        ["page"] = page.ToString(),
                        ["per_page"] = _productsPerPage,
                    }));
                    page += 1;
                }
                catch (Exception e)
                {
                    _logger.LogError("Failed to retrieve products page", e);
                    
                    await Task.Delay(Consts.FailedRequestDelay);
                }

                await Task.Delay(Consts.RequestDelay);
            }

            return products.Select(product => new Model.Entities.Product
            {
                Id = int.TryParse(product.sku, out int id) ? id : -1,
                Name = product.name,
                Availability = product.meta_data
                    .Where(meta => meta.key == _metaKey)
                    .SelectMany(meta => ((string) meta.value).Split(_separator))
                    .Select(storeName => new Store
                    {
                        Name = storeName,
                        Quantity = product.stock_status == Consts.AvailableStatus ? 1 : 0,
                        Type = StoreType.Shop,
                    })
                    .ToList()
            })
            .ToList();
        }

        private async Task<Product> GetProductBySku(string sku)
        {
            var products = await WcClient.Product.GetAll(new Dictionary<string, string>
            { 
                { "sku", sku },
            });

            return products.FirstOrDefault() ?? throw new Exception($"Product with sku {sku} was not found");
        }

        private void ResponseFilter(HttpWebResponse response)
        {
            _totalPages = int.Parse(response.Headers["X-WP-TotalPages"]);
        }

        private WCObject Connect() 
        {
            RestAPI rest = new RestAPI(
                $"{_configuration["WcHost"]}/wp-json/wc/v3/", 
                _configuration["WcClient"], 
                _configuration["WcSecret"],
                responseFilter: ResponseFilter);

            return new WCObject(rest);
        }
    }
}
