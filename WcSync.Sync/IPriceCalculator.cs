using WcSync.Model.Entities;

namespace WcSync.Sync
{
    public interface IPriceCalculator
    {
        (decimal? price, decimal? salePrice) GetPrice(DbProduct product);
    }
}