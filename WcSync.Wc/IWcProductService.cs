using System.Collections.Generic;
using System.Threading.Tasks;
using WcSync.Model.Entities;

namespace WcSync.Wc 
{
    public interface IWcProductService
    {
        Task UpdateProduct(string sku, string stockStatus, string availability);

        Task UpdateProduct(int productId, string stockStatus, string availability, decimal? price);

        Task<List<WcProduct>> GetProductsAsync();
    }
}