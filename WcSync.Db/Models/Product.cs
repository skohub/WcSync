using System.Collections.Generic;

namespace WcSync.Db.Models
{
    public class Product 
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public IList<Store> Availability { get; set; }
    }
}