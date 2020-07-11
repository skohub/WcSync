using System.Linq;
using Microsoft.Extensions.Logging;
using WcSync.Model.Entities;

namespace WcSync.Sync
{
    public class PriceCalculator : IPriceCalculator
    {
        private readonly ILogger _logger;

        private const decimal _discount = 0.97m;

        public PriceCalculator(ILogger<ProductService> logger)
        {
            _logger = logger;
        }

        public decimal? GetPrice(DbProduct product)
        {
            if (product.Availability?.Any() != true)
            {
                return null;
            }

            decimal price = product.Availability.First().Price;


            if (product.Availability.All(s => s.Price == price)) 
            {
                return ApplyDiscount(price);
            }
            else
            {
                var prices = product.Availability.Select(a => $"{a.Name: a.Price}");
                _logger.LogInformation($"Prices are not equal in stores. {string.Join(", ", prices)}");

                return null;
            }
        }

        private decimal ApplyDiscount(decimal price)
        {
            return decimal.Round((price * _discount) / 10) * 10;
        }
    }
}