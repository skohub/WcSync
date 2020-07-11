using WcSync.Model.Entities;

namespace WcSync.Db
{
    public class ItemRestDto
    {
        public int ItemID { get; set; }

        public string i_n { get; set; }

        public string name { get; set; }

        public StoreType StoreType { get; set; }

        public int summ { get; set; }    

        public decimal price { get; set; }
    }
}