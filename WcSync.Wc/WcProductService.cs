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

        public async Task UpdateProduct(string sku, string stockStatus, string availability)
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
                        value = availability,
                    }
                }
            });
        }

        public async Task UpdateProduct(int productId, string stockStatus, string availability, decimal? price)
        {
            await WcClient.Product.Update(productId, new Product { 
                stock_status = stockStatus,
                price = price,
                regular_price = price,
                meta_data = new List<ProductMeta>
                {
                    new ProductMeta 
                    {
                        key = _metaKey,
                        value = availability,
                    }
                }
            });
        }

        public async Task<List<WcProduct>> GetProductsAsync()
        {
            var products = new List<Product>();
            var page = 1;
            _totalPages = 1; // Until we figure out by parsing response headers

            // Keep requests single threaded to prevent WooCommerce server overload
            while (page <= _totalPages)
            {
                _logger.LogInformation($"Retrieveing products, page {page}/{(_totalPages == 1 ? "?" : _totalPages.ToString())}");

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

            return products
                .Where(p => p.id != null)
                .Where(p => int.TryParse(p.sku, out _))
                .Select(product => new WcProduct
                {
                    Id = product.id.Value,
                    Sku = product.sku,
                    Name = product.name,
                    Availability = (string) product.meta_data.FirstOrDefault(meta => meta.key == _metaKey)?.value,
                    Price = product.price,
                    StockStatus = product.stock_status,
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
            int.TryParse(response.Headers["X-WP-TotalPages"], out _totalPages);
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
