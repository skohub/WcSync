using System.Threading.Tasks;

namespace WcSync.Model
{
    public interface IProductService
    {
        Task UpdateAllProductsAsync();

        Task ListProductsDicrepancies();
    }
}