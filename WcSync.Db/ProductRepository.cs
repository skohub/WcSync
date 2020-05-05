using System;
using System.Collections.Generic;
using WcSync.Db.Models;

namespace WcSync.Db
{
    public class ProductRepository : IProductRepository
    {
        public IList<Product> GetAvailableProducts()
        {
            throw new NotImplementedException();
        }
    }
}
