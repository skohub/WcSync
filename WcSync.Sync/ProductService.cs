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

                    if (dbProduct == null) 
                    {
                        await SetUnavailableStatus(wcProduct);

                        continue;
                    }

                    if (ProductEquals(wcProduct, dbProduct) == false)
                    {
                        await UpdateProduct(wcProduct, dbProduct);
                    }
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

            foreach (var wcProduct in wcProducts.Where(p => p.Availability?.Any() == true))
            {
                var dbProduct = dbProducts.FirstOrDefault(p => int.TryParse(wcProduct.Sku, out int id) && p.Id == id);
                if (dbProduct == null) 
                {
                    continue;
                }

                (var price, var salePrice) = _priceCalculator.GetPrice(dbProduct);
                if (price == null) 
                {
                    continue;
                }

                if (wcProduct.SalePrice != salePrice)
                {
                    _logger.LogInformation($"{wcProduct.Name} - {wcProduct.Sku}. Site/database: {salePrice} / {price}");
                }
            }

            var notFoundProducts = dbProducts
                .Where(dbProduct => !wcProducts.Any(wcProduct => int.TryParse(wcProduct.Sku, out int id) && dbProduct.Id == id))
                .ToList();

            var notFoundProductsStr = string.Join("\r\n", notFoundProducts.Select(product => $"{product.Name} - {product.Id}"));

            notFoundProducts.ForEach(product => _logger.LogInformation($"Not found product: {product.Name} - {product.Id}"));
        }

        private bool ProductEquals(WcProduct wcProduct, DbProduct dbProduct)
        {
            return 
                wcProduct.StockStatus == dbProduct.GetStockStatus() &&
                wcProduct.Availability == dbProduct.GetAvailability() &&
                ProductPriceEquals(wcProduct, dbProduct);
        }

        private bool ProductPriceEquals(WcProduct wcProduct, DbProduct dbProduct)
        {
            if (wcProduct.FixedPrice)
            {
                _logger.LogInformation($"Product {wcProduct.Name} - {wcProduct.Sku} has fixed price.");

                return true;
            }

            (var price, var salePrice) = _priceCalculator.GetPrice(dbProduct);

            if (price == null) return true;

            return 
                wcProduct.RegularPrice == price && 
                (wcProduct.SalePrice ?? price) == salePrice;
        }

        private async Task UpdateProduct(WcProduct wcProduct, DbProduct dbProduct) 
        {
            if (wcProduct == null) 
            {
                _logger.LogError($"{nameof(wcProduct)} should not be null");
            }

            try
            {
                var stockStatus = dbProduct.GetStockStatus();
                var availability = dbProduct.GetAvailability();
                (var price, var salePrice) = _priceCalculator.GetPrice(dbProduct);

                if (wcProduct.FixedPrice)
                {
                    price = wcProduct.RegularPrice;
                    salePrice = wcProduct.SalePrice;
                }

                _logger.LogInformation(
                    $"Updating product {wcProduct.Name} - {wcProduct.Sku} from {wcProduct.StockStatus} - " +
                    $"\"{wcProduct.Availability}\" price: {wcProduct.RegularPrice:F0}/{wcProduct.SalePrice:F0} to " +
                    $"{stockStatus} - \"{availability}\", price: {price:F0}/{salePrice:F0}");
                await _wcProductService.UpdateProduct(wcProduct.Id, stockStatus, availability, price, salePrice);

                await Task.Delay(Consts.RequestDelay);
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

        private async Task SetUnavailableStatus(WcProduct wcProduct)
        {
            if (wcProduct.StockStatus != Consts.UnavailableStatus || !string.IsNullOrWhiteSpace(wcProduct.Availability))
            {
                _logger.LogInformation($"Updating product {wcProduct.Name} - {wcProduct.Sku} to \"{Consts.UnavailableStatus}\"");
                await _wcProductService.UpdateProduct(wcProduct.Id, Consts.UnavailableStatus, string.Empty, null, null);

                await Task.Delay(Consts.RequestDelay);
            }
        }
    }
}