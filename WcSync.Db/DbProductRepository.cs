using System.Linq;
using System.Collections.Generic;
using WcSync.Db.Models;
using Microsoft.Extensions.Configuration;
using Dapper;
using MySql.Data.MySqlClient;

namespace WcSync.Db
{
    public class DbProductRepository : IDbProductRepository
    {
        private IConfiguration _configuration;
        private MySqlConnection _connection;

        private MySqlConnection Connection => 
            _connection ?? (_connection = new MySqlConnection(_configuration["ConnectionString"]));

        public DbProductRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public List<Product> GetAvailableProducts()
        {
            // get a flat list of products and group by productid
            return Connection
                .Query<FlatProductDto>(
                    sql: "call recently_updated_items(?)",
                    param: new { days_offset = 1 })
                .GroupBy(
                    fp => fp.ProductId, 
                    fp => fp,
                    (id, products) => new Product
                    {
                        Id = id,
                        Name = products.First(p => p.ProductId == id).ProductName,
                        Availability = products
                            .Select(p => new Store
                            {
                                Name = p.StoreName,
                                Number = p.Number,
                                Type = p.StoreType,
                            })
                            .ToList(),
                    })
                .ToList();
        }
    }
}
