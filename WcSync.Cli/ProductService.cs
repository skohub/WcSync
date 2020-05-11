using System;
using System.Linq;
using WcSync.Wc;
using WcSync.Db;
using WcSync.Db.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WcSync.Cli
{
    public class ProductService : IProductService
    {
        private readonly IWcProductService _wcProductService;
        private readonly IDbProductRepository _dbProductRepository;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IWcProductService wcProductService, 
            IDbProductRepository dbProductRepository,
            ILogger<ProductService> logger)
        {
            _wcProductService = wcProductService;
            _dbProductRepository = dbProductRepository;
            _logger = logger;
        }

        public void UpdateRecentProducts()
        {
            _logger.LogDebug($"Begin {nameof(UpdateRecentProducts)}");

            try
            {
                var products = _dbProductRepository.GetAvailableProducts();

                _logger.LogInformation($"Found {products.Count} product(s) to update");

                var tasks = products.Select(UpdateProduct).ToArray();
                Task.WaitAll(tasks);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong");
            }

            _logger.LogDebug($"End {nameof(UpdateRecentProducts)}");
        }

        private async Task UpdateProduct(Product product)
        {
            if (product == null) throw new Exception($"{nameof(product)} should not be null");
            
            int? id = null;
            try
            {
                id = await _wcProductService.GetProductIdBySku(product.Id.ToString());
            } 
            catch (Exception e) 
            {
                _logger.LogError(e, $"Product {product.Name} - {product.Id} was not found in woocommerce");
            }

            try
            {
                if (id.HasValue) {
                    var newStockStatus = GetStockStatus(product);
                    await _wcProductService.SetStockStatus(id.Value, newStockStatus);

                    _logger.LogInformation($"Product {product.Name} - {product.Id} is successfully updated to \"{newStockStatus}\"");
                }
                else
                {
                    _logger.LogInformation($"Skipped {nameof(_wcProductService.SetStockStatus)} because of absent product id");
                }
            } 
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to {nameof(_wcProductService.SetStockStatus)}");
            }            
        }

        private string GetStockStatus(Product product) 
        {
            return product.Availability.Sum(a => a.Number) > 0 ? "instock" : "onbackorder";
        }
    }
}