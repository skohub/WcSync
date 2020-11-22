using System.Collections.Generic;
using System.Threading.Tasks;
using WcSync.Model.Entities;

namespace WcSync.Wc 
{
    public interface IWcProductService
    {
        Task UpdateProduct(int productId, string stockStatus, string availability, decimal? regularPrice, decimal? salePrice);

        Task<List<WcProduct>> GetProductsAsync();
    }
}