using WcSync.Model.Entities;

namespace WcSync.Sync
{
    public interface IPriceCalculator
    {
        decimal? GetPrice(DbProduct product);
    }
}