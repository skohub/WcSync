using System.Linq;
using Microsoft.Extensions.Logging;
using WcSync.Model.Entities;

namespace WcSync.Sync
{
    public class PriceCalculator : IPriceCalculator
    {
        private readonly ILogger _logger;

        private const decimal _discount = 0.97m;

        private const decimal _discuontLowerBoundary = 3000m;

        public PriceCalculator(ILogger<ProductService> logger)
        {
            _logger = logger;
        }

        public (decimal? price, decimal? salePrice) GetPrice(DbProduct product)
        {
            if (product?.Availability?.Any() != true) return (null, null);

            var availability = product.Availability
                .Where(a => a.Type == StoreType.Shop || a.Type == StoreType.Warehouse)
                .Where(a => a.Quantity > 0)
                .Where(a => a.Price > 0)
                .ToList();

            if (availability.Any() != true) return (null, null);

            var price = decimal.Round(availability.First().Price);

            if (availability.All(s => s.Price == price) == false)
            {
                price = availability.Max(a => a.Price);
                var prices = availability.Select(a => $"{a.Name}: {a.Price}");
                _logger.LogInformation($"Prices are not equal in stores for {product.Name} - {product.Id}. {string.Join(", ", prices)}");
            }

            return (price, price);
        }

        private decimal ApplyDiscount(decimal price)
        {
            if (price <= _discuontLowerBoundary)
            {
                return price;
            }

            return decimal.Round((price * _discount) / 10) * 10;
        }
    }
}