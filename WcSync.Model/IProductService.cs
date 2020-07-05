using System.Threading.Tasks;

namespace WcSync.Model
{
    public interface IProductService
    {
        Task UpdateRecentProductsAsync();

        Task UpdateAllProductsAsync();
    }
}