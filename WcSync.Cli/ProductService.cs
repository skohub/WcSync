using System;
using System.Linq;
using WcSync.Wc;
using WcSync.Db;
using WcSync.Db.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

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

            try
            {
                var stockStatus = GetStockStatus(product);
                var availability = GetProductAvailability(product);
                await _wcProductService.UpdateStockStatus(product.Id.ToString(), stockStatus, availability);

                _logger.LogInformation($"Product {product.Name} - {product.Id} was successfully updated to \"{stockStatus}\"");
            } 
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to {nameof(_wcProductService.UpdateStockStatus)} for {product.Name} - {product.Id}");
            }            
        }

        private string GetStockStatus(Product product) 
        {
            return product.Availability.Sum(a => a.Number) > 0 ? "instock" : "onbackorder";
        }

        private IList<string> GetProductAvailability(Product product)
        {
            return product.Availability.Where(a => a.Type == StoreType.Shop).Select(a => a.Name).ToList();
        }
    }
}