using System.Threading.Tasks;

namespace WcSync.Wc 
{
    public interface IWcProductService
    {
        Task<int> GetProductIdBySku(string sku);

        Task SetStockStatus(int id, string stockStatus);
    }
}