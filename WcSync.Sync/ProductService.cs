using System;
using System.Linq;
using WcSync.Wc;
using WcSync.Db;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Net;
using WcSync.Model;
using WcSync.Model.Entities;

namespace WcSync.Sync
{
    public class ProductService : IProductService
    {
        private readonly IWcProductService _wcProductService;
        private readonly IDbProductRepository _dbProductRepository;
        private readonly IPriceCalculator _priceCalculator;
        private readonly ILogger<ProductService> _logger;

        public ProductService(
            IWcProductService wcProductService, 
            IDbProductRepository dbProductRepository,
            IPriceCalculator priceCalculator,
            ILogger<ProductService> logger)
        {
            _wcProductService = wcProductService;
            _dbProductRepository = dbProductRepository;
            _priceCalculator = priceCalculator;
            _logger = logger;
        }

        public async Task UpdateRecentProductsAsync()
        {
            _logger.LogDebug($"Begin {nameof(UpdateRecentProductsAsync)}");

            try
            {
                var products = _dbProductRepository.GetRecentlyUpdatedProducts();

                _logger.LogInformation($"Found {products.Count} product(s) to update");

                foreach (var product in products)
                {
                    await Task.Delay(Consts.RequestDelay);
                    await UpdateProduct(product);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong");
            }

            _logger.LogDebug($"End {nameof(UpdateRecentProductsAsync)}");
        }

        public async Task UpdateAllProductsAsync()
        {
            _logger.LogDebug($"Begin {nameof(UpdateAllProductsAsync)}");

            try
            {
                var dbProducts = _dbProductRepository.GetProducts();
                var wcProducts = await _wcProductService.GetProductsAsync();

                foreach (var wcProduct in wcProducts)
                {
                    var dbProduct = dbProducts.FirstOrDefault(p => int.TryParse(wcProduct.Sku, out int id) && p.Id == id);

                    await UpdateProductIfNecessary(wcProduct, dbProduct);
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Something went wrong");
            }

            _logger.LogDebug($"End {nameof(UpdateAllProductsAsync)}");
        }

        public async Task ListPriceDicrepancies()
        {
            var dbProducts = _dbProductRepository.GetProducts();
            var wcProducts = await _wcProductService.GetProductsAsync();

            foreach (var wcProduct in wcProducts)
            {
                var dbProduct = dbProducts.FirstOrDefault(p => int.TryParse(wcProduct.Sku, out int id) && p.Id == id);
                if (dbProduct == null) 
                {
                    continue;
                }

                var price = _priceCalculator.GetPrice(dbProduct);
                if (price == null) 
                {
                    continue;
                }

                if (wcProduct.Price != price)
                {
                    _logger.LogInformation($"{wcProduct.Name} - {wcProduct.Sku}. Site/database: {wcProduct.Price} / {price}");
                }
            }
        }

        private async Task UpdateProductIfNecessary(WcProduct wcProduct, DbProduct dbProduct) 
        {
            if (wcProduct == null) 
            {
                _logger.LogError($"{nameof(wcProduct)} should not be null");

                return;
            }

            try
            {
                string stockStatus = Consts.UnavailableStatus;
                string availability = null;
                decimal? price = null;

                if (dbProduct != null) 
                {
                    stockStatus = dbProduct.GetStockStatus();
                    availability = dbProduct.GetAvailability();
                    price = _priceCalculator.GetPrice(dbProduct);
                }

                bool priceUpdated = price != null && (wcProduct.Price != price);

                if (wcProduct.StockStatus != stockStatus || wcProduct.Availability != availability || priceUpdated)
                {
                    await _wcProductService.UpdateProduct(wcProduct.Id, stockStatus, availability, price);

                    _logger.LogInformation(
                        $"Product {wcProduct.Name} - {wcProduct.Sku} was successfully updated to \"{stockStatus}\", " +
                        $"available in \"{availability}\", price: {price}");

                    await Task.Delay(Consts.RequestDelay);
                }
            } 
            catch (WebException e)
            {
                _logger.LogError(e, $"Failed to {nameof(_wcProductService.UpdateProduct)} for {wcProduct.Name} - {wcProduct.Sku}");
                await Task.Delay(Consts.FailedRequestDelay);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to {nameof(_wcProductService.UpdateProduct)} for {wcProduct.Name} - {wcProduct.Sku}");
            }
        }

        private async Task UpdateProduct(DbProduct product)
        {
            if (product == null) throw new Exception($"{nameof(product)} should not be null");

            try
            {
                var stockStatus = product.GetStockStatus();
                var availability = product.GetAvailability();
                await _wcProductService.UpdateProduct(product.Id.ToString(), stockStatus, availability);

                _logger.LogInformation(
                    $"Product {product.Name} - {product.Id} was successfully updated to \"{stockStatus}\", " +
                    $"available in \"{availability}\"");
            } 
            catch (WebException e)
            {
                _logger.LogError(e, $"Failed to {nameof(_wcProductService.UpdateProduct)} for {product.Name} - {product.Id}");
                await Task.Delay(Consts.FailedRequestDelay);
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Failed to {nameof(_wcProductService.UpdateProduct)} for {product.Name} - {product.Id}");
            }
        }
    }
}