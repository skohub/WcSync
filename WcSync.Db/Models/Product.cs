using System.Collections.Generic;

namespace WcSync.Db.Models
{
    public class Product 
    {
        public int Id { get; set; }

        public IList<Store> Availability { get; set; }
    }
}