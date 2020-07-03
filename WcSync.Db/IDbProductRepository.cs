using System.Collections.Generic;
using WcSync.Model.Entities;

namespace WcSync.Db
{
    public interface IDbProductRepository
    {
        List<Product> GetAvailableProducts();
    }
}