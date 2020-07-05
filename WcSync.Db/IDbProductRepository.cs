using System.Collections.Generic;
using WcSync.Model.Entities;

namespace WcSync.Db
{
    public interface IDbProductRepository
    {
        List<DbProduct> GetRecentlyUpdatedProducts();

        List<DbProduct> GetProducts();
    }
}