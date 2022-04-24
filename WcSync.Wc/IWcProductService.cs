using System.Collections.Generic;
using System.Threading.Tasks;
using WcSync.Model.Entities;

namespace WcSync.Wc 
{
    public interface IWcProductService
    {
        Task UpdateProductAsync(int productId, string stockStatus, string availability, decimal? regularPrice, decimal? salePrice);
        Task UpdateProductsAsync(List<WcProduct> products);
        Task<List<WcProduct>> GetProductsAsync();
    }
}