using System.Data.SQLite;
using System.Collections.Generic;

namespace Cryptothune.Lib
{
    public interface IExchange
    {
        AssetSymbol NormalizeSymbolName(string symbol);

        double Fees(double whole, Trade.TOrderType oType);

        Dictionary<string, decimal> Balances();

        double Balance(string asset);

        IEnumerable<double> PricesHistory(AssetSymbol assetName);

        double MarketPrice(AssetSymbol assetName);
        
        Trade LatestTrade(AssetSymbol assetName);

        bool Buy (AssetSymbol assetName, double price, double qty = 100.0, bool dry = true);
        bool Sell (AssetSymbol assetName, double price, double qty = 100.0, bool dry = true);
        void PreventRateLimit();
    }
}
