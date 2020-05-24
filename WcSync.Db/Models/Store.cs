namespace WcSync.Db.Models
{
    public class Store 
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        public StoreType Type { get; set; }
    }
}