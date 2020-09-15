using System.Data.SQLite;
using System.Collections.Generic;

namespace Cryptothune.Lib
{
    public interface IExchange
    {
        AssetName NormalizeSymbolName(string symbol);

        double Fees(double whole, Trade.TOrderType oType);

        Dictionary<string, decimal> Balances();

        double Balance(string asset);

        IEnumerable<double> PricesHistory(AssetName assetName);

        double MarketPrice(AssetName assetName);
        
        Trade LatestTrade(AssetName assetName);

        bool Buy (AssetName assetName, double price, double qty = 100.0, bool dry = true);
        bool Sell (AssetName assetName, double price, double qty = 100.0, bool dry = true);
        void PreventRateLimit();
    }
}
