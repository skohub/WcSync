namespace WcSync.Model.Entities
{
    public class Store 
    {
        public string Name { get; set; }

        public int Quantity { get; set; }

        public decimal Price { get; set; }

        public StoreType Type { get; set; }
    }
}