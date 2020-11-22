using System.Collections.Generic;
using System.Linq;

namespace WcSync.Model.Entities
{
    public class WcProduct 
    {
        public int Id { get; set; }

        public string Sku { get; set; }

        public string Name { get; set; }

        public string Availability { get; set; }

        public decimal? RegularPrice { get; set; }

        public decimal? SalePrice { get; set; }

        public string StockStatus { get; set; }

        public bool FixedPrice { get; set; }
    }
}