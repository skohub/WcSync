using System.Collections.Generic;
using System.Threading.Tasks;

namespace WcSync.Wc 
{
    public interface IWcProductService
    {
        Task UpdateStockStatus(string sku, string stockStatus, IList<string> availability);
    }
}