using System.Data.SQLite;
using System.Collections.Generic;

namespace Cryptothune.Lib
{
    public interface IExchange
    {
        string Name();

        double Fees(double whole);

        Dictionary<string, decimal> GetBalances();

        SQLiteConnection ExportTradesOnDB(string symbol);

        double GetMarketPrice(string symbol);
        
        Trade GetLatestTrade(string symbol);

        void Buy(string symbol, double price, bool dry);

        void Sell(string symbol, double price, bool dry);
    }
}
