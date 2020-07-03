using WcSync.Model.Entities;

namespace WcSync.Db
{
    public class FlatProductDto
    {
        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string StoreName { get; set; }

        public StoreType StoreType { get; set; }

        public int Quantity { get; set; }    
    }
}