using System;
using System.Linq;
using WcSync.Wc;
using WcSync.Db;
using WcSync.Db.Models;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace WcSync.Cli
{
    public class ProductService : IProductService
    {
        private const int RequestDelay = 3000;
        private const int FailedRequestDelay = 60000;

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

        public async Task UpdateRecentProductsAsync()
        {
            _logger.LogDebug($"Begin {nameof(UpdateRecentProductsAsync)}");

            try
            {
                var products = _dbProductRepository.GetAvailableProducts();

                _logger.LogInformation($"Found {products.Count} product(s) to update");

                foreach (var product in products)
                {
                    await Task.Delay(RequestDelay);
                    await UpdateProduct(product);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong");
            }

            _logger.LogDebug($"End {nameof(UpdateRecentProductsAsync)}");
        }

        private async Task UpdateProduct(Product product)
        {
            if (product == null) throw new Exception($"{nameof(product)} should not be null");

            try
            {
                var stockStatus = GetStockStatus(product);
                var availability = GetProductAvailability(product);
                await _wcProductService.UpdateStockStatus(product.Id.ToString(), stockStatus, availability);

                _logger.LogInformation(
                    $"Product {product.Name} - {product.Id} was successfully updated to \"{stockStatus}\", " +
                    $"available in \"{string.Join(", ", availability)}\"");
            } 
            catch (WebException e)
            {
                _logger.LogError(e, $"Failed to {nameof(_wcProductService.UpdateStockStatus)} for {product.Name} - {product.Id}");
                await Task.Delay(FailedRequestDelay);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to {nameof(_wcProductService.UpdateStockStatus)} for {product.Name} - {product.Id}");
            }
        }

        private string GetStockStatus(Product product) 
        {
            var available = product.Availability
                .Where(a => a.Type == StoreType.Shop || a.Type == StoreType.Warehouse)
                .Any(a => a.Quantity > 0);

            return available ? "instock" : "onbackorder";
        }

        private IList<string> GetProductAvailability(Product product)
        {
            return product.Availability
                .Where(a => a.Type == StoreType.Shop)
                .Where(a => a.Quantity > 0)
                .Select(a => a.Name).ToList();
        }
    }
}