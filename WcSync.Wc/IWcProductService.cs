using System.Collections.Generic;
using System.Threading.Tasks;
using WcSync.Model.Entities;

namespace WcSync.Wc 
{
    public interface IWcProductService
    {
        Task UpdateStockStatus(string sku, string stockStatus, string availability);

        Task UpdateStockStatus(int productId, string stockStatus, string availability);

        Task<List<WcProduct>> GetProductsAsync();
    }
}