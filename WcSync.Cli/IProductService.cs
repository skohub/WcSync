using System.Threading.Tasks;

namespace WcSync.Cli
{
    public interface IProductService
    {
        Task UpdateRecentProductsAsync();
    }
}