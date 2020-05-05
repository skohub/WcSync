using System;
using System.Linq;
using WcSync.Wc;
using WcSync.Db;
using WcSync.Db.Models;
using System.Threading.Tasks;

namespace WcSync.Cli
{
    public class ProductService : IProductService
    {
        private readonly IWcProductService _wcProductService;
        private readonly IDbProductRepository _dbProductRepository;

        public ProductService(IWcProductService wcProductService, IDbProductRepository dbProductRepository)
        {
            _wcProductService = wcProductService;
            _dbProductRepository = dbProductRepository;
        }

        public void UpdateRecentProducts()
        {
            var tasks = _dbProductRepository.GetAvailableProducts().Select(UpdateProduct).ToArray();
            Task.WaitAll(tasks);
        }

        private async Task UpdateProduct(Product product)
        {
            if (product == null) throw new Exception($"{nameof(product)} should not be null");
            
            var id = await _wcProductService.GetProductIdBySku(product.Id.ToString());
            await _wcProductService.SetStockStatus(id, GetStockStatus(product));
        }

        private string GetStockStatus(Product product) 
        {
            return product.Availability.Sum(a => a.Number) > 0 ? "instock" : "onbackorder";
        }
    }
}