using System.Data.SQLite;
using System.Collections.Generic;

namespace Cryptothune.Lib
{
    public interface IExchange
    {
        double Fees(double whole, Trade.TOrderType oType);

        Dictionary<string, decimal> Balances();

        double Balance(string asset);

        IEnumerable<double> PricesHistory(string symbol);

        double MarketPrice(string symbol);
        
        Trade LatestTrade(string symbol);

        bool Buy (string symbol, double price, double qty = 100.0, bool dry = true);
        bool Sell (string symbol, double price, double qty = 100.0, bool dry = true);
    }
}
