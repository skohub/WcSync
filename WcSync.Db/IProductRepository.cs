using System.Collections.Generic;
using WcSync.Db.Models;

namespace WcSync.Db
{
    public interface IProductRepository
    {
        IList<Product> GetAvailableProducts();
    }
}