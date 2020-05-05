using System.Collections.Generic;
using WcSync.Db.Models;

namespace WcSync.Db
{
    public interface IDbProductRepository
    {
        List<Product> GetAvailableProducts();
    }
}