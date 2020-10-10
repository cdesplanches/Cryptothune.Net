using System;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;

namespace CryptoThune.Net
{
    /// <summary>
    /// A 'fake' exchqnge market, built on top of the kraken public exchange market.
    /// </summary>
    public class ExchangeFake : ExchangeKraken
    {
        /// <summary>
        /// The current amount of money on the 'fake' portfolio
        /// </summary>
        /// <value></value>
        private double _money { get; set; }
        /// <summary>
        /// Internal Database
        /// </summary>
        private SQLiteConnection _fakeDB;
        /// <summary>
        /// Store the market price history
        /// </summary>
        private Dictionary<string, Dictionary<double, double>> _priceHistory = new Dictionary<string, Dictionary<double, double>>();
        /// <summary>
        /// the list of trades.
        /// </summary>
        /// <returns></returns>
        private List<Trade> _tradeHistory = new List<Trade>();
        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, double> _assetPortfolio = new Dictionary<string, double>();
        /// <summary>
        /// ctor
        /// </summary>
        public ExchangeFake()
        {
            string cs = @"URI=file:ExchangeFake.db";
            _fakeDB = new SQLiteConnection(cs);
            _fakeDB.Open();

            var cmd = new SQLiteCommand(_fakeDB);
            cmd.CommandText = @"DROP TABLE IF EXISTS Assets";
            cmd.ExecuteNonQuery();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS Assets (asset text PRIMARY KEY, balance double, unique (asset))";
            cmd.ExecuteNonQuery();
            cmd.CommandText = @"DROP TABLE IF EXISTS Trades";
            cmd.ExecuteNonQuery();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS 'Trades' (my_id integer PRIMARY KEY, timestamp integer, symbol text, order_type text, price double, amount double, unique (timestamp,symbol))";
            cmd.ExecuteNonQuery();
        }
        /// <summary>
        /// Get the total balances of the virtual portfolio
        /// </summary>
        /// <returns></returns>
        public override Dictionary<string, decimal> Balances(DateTime? dt = null)
        {
            var dd = dt ?? DateTime.Now;

            _money = 0;
            var oa = dd.ToOADate();
            var dic = new Dictionary<string, decimal>();
            foreach ( var ass in _assetPortfolio )
            {
                double bal = ass.Value;;
                if ( _priceHistory.ContainsKey(ass.Key) )
                {
                    var keys = _priceHistory[ass.Key].Keys;
                    if (  _priceHistory[ass.Key].Keys.First() <= oa )
                    {
                        var nearest = oa - keys.Where(k => k <= oa)
                                                .Min(k => oa - k);
                        var prc = _priceHistory[ass.Key][nearest];
                        _money += prc * ass.Value;
                    }
                }
                else
                {
                    _money += ass.Value;
                }

                dic.Add(ass.Key, (decimal)bal);
            }

            RateLimiterPenality += 6000;
            return dic;
        }
        /// <summary>
        /// Get the current available balance (from the DB)
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        public override double Balance(string asset)
        {
            RateLimiterPenality += 6000;
            return _money;
//            return _assetPortfolio[asset];
        }
        /// <summary>
        /// Fake a deposit order (update the entry on the sqlite DB)
        /// </summary>
        /// <param name="money"></param>
        /// <returns></returns>
        public virtual double Deposit(double money)
        {
            _money += money;
            _assetPortfolio["ZEUR"] = _money;
            return _money;
        }
        /// <summary>
        /// The name of this Fake market exchange
        /// </summary>
        /// <returns></returns>
        public override string Name()
        {
            return "Fake";
        }
        /// <summary>
        /// Return the normalized name for a given symbol
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        public override AssetSymbol NormalizeSymbolName(string symbol)
        {
            var baseName = symbol.Substring(0, 3);
            var quote = "Z" + symbol.Substring(symbol.Length-3, 3);

            _assetPortfolio.Add(baseName, 0);

            return new AssetSymbol(symbol, baseName, quote);
        }
        /// <summary>
        /// Get the prices history for a given asset
        /// </summary>
        /// <param name="assetName">The nqme of the asset to get the price history</param>
        /// <returns>list of prices</returns>
        public override Dictionary<double, double> PricesHistory(AssetSymbol assetName)
        {
            var con = ExportTradesOnDB(assetName);

            // Get all prices
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = @"SELECT timestamp, price FROM history ORDER BY timestamp";
            cmd.ExecuteNonQuery();

            using SQLiteDataReader rdr = cmd.ExecuteReader();
            var _dict = new Dictionary<double, double>();
            while (rdr.Read())
            {
                var timestamp = (Int64)rdr["timestamp"];
                var dt = new DateTime(timestamp);
                var oa = (double)dt.ToOADate();

                var r = (double)rdr["price"];

                if ( _dict.ContainsKey(oa) )
                    _dict[oa] = r;
                else
                    _dict.Add(oa, r);

            }
            _priceHistory.Add(assetName.BaseName, _dict);
            return _dict;
        }
        /// <summary>
        /// Get the price history for a given asset and store it into a sqlite DB.
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public SQLiteConnection ExportTradesOnDB(AssetSymbol assetName)
        {
            string cs = @"URI=file:" + assetName.SymbolName + ".db";
            var con = new SQLiteConnection(cs);
            con.Open();
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS history (my_id integer PRIMARY KEY, timestamp integer, price double, amount double, unique (timestamp,price,amount))";
            cmd.ExecuteNonQuery();
            cmd.CommandText = @"CREATE TABLE IF NOT EXISTS lastcheck (timestamp integer)";
            cmd.ExecuteNonQuery();

            
            cmd.CommandText = @"SELECT COUNT(*) FROM history";
            var count = Convert.ToInt32(cmd.ExecuteScalar());
            var dt = new DateTime(2008, 1, 1, 0, 0, 0);
            if (count != 0)
            {
                cmd.CommandText = @"SELECT * FROM lastcheck";
                using (SQLiteDataReader rdr = cmd.ExecuteReader())
                {
                    if (rdr.HasRows)
                    {
                        if (rdr.Read())
                        {
                            var timestamp = rdr.GetInt64(0);
                            dt = new DateTime(timestamp);
                        }
                    }
                }
            }
            else
            {
                cmd.CommandText = @"INSERT INTO lastcheck (timestamp) VALUES (@timestamp)";
                cmd.Parameters.AddWithValue("@timestamp", dt.Ticks);
                cmd.Prepare();
                cmd.ExecuteNonQuery();
            }

            bool fullyUpdated = false;
            while (!fullyUpdated)
            {
                var trades = TradesHistory(assetName, dt);
                using (var transaction = con.BeginTransaction())
                {
                    foreach (var it in trades.Data)
                    {
                        cmd.CommandText = @"INSERT OR IGNORE INTO history(timestamp, price, amount) VALUES (@timestamp, @price, @amount)";
                        cmd.Parameters.AddWithValue("@timestamp", it.Timestamp.Ticks);
                        cmd.Parameters.AddWithValue("@price", it.Price);
                        cmd.Parameters.AddWithValue("@amount", it.Quantity);
                        cmd.Prepare();
                        cmd.ExecuteNonQuery();
                    }

                    transaction.Commit();
                }


                dt = trades.Last;
                cmd.CommandText = @"UPDATE lastcheck SET timestamp = '" + dt.Ticks + "'";
                cmd.ExecuteNonQuery();


                if (trades.Data.Count() < 1000)
                {
                    fullyUpdated = true;
                }

                base.PreventRateLimit(); // To avoid a rate limit exception on Kraken API public calls.
            }

            return con;
        }
        /// <summary>
        /// Override of the PreventRateLimit
        /// </summary>
        /// <returns></returns>
        public override int PreventRateLimit()
        {
            var ms = RateLimiterPenality+1000;      // Add a default penality
            RateLimiterPenality = 0;
            return ms;
        }
        /// <summary>
        /// Get the latest transaction performed for a given asset
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public override Trade LatestTrade(AssetSymbol assetName)
        {
            if ( _tradeHistory.Count == 0 )
            {
                var tr = new Trade();
                tr.Asset = assetName;
                return tr;
            }
            var hist = from h in _tradeHistory
                        where h.Asset.SymbolName == assetName.SymbolName
                        orderby h.Timestamp descending
                        select h;
            RateLimiterPenality += 6000;
            if ( hist.Count() == 0 )
            {
                var tr = new Trade();
                tr.Asset = assetName;
                return tr; 
            }
                
            return hist.First();
        }
        /// <summary>
        /// Place a 'fake' buy order
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="price"></param>
        /// <param name="ratio"></param>
        /// <param name="dt"></param>
        /// <param name="dry"></param>
        /// <returns></returns>
        public override bool Buy(AssetSymbol assetName, double price, double ratio, DateTime? dt, bool dry)
        {
            if ( ratio == 0 )
            {
                return false;
            }
            var dd = dt ?? DateTime.Now; 

            var fiatSymbol = assetName.QuoteName;                   // "XRPEUR" => "ZEUR"
            var assetSymbol = assetName.SymbolName;
            var bal = Balances(dd);
            var availBalance = (double)bal[fiatSymbol];             // Available money for trading.
            var totalBalance = Balance(fiatSymbol);                 // Total balance of the portfolio on this exchange market
            var amount = (totalBalance*ratio)/100.0;                // Percentage of this total balance that can be used to p
            if ( amount > availBalance)
            {
                amount = availBalance;
            }

            var qty = amount / price;
            if ( qty >= assetName.OrderMin )
            {
                NLog.LogManager.GetCurrentClassLogger().Info( dd + "|Place Order: Buy (" + assetSymbol + ") - Quantity: " + qty + " - Price:" + price + " - Total: " + amount + " " + assetName.QuoteName );
                var success = PlaceOrder(assetName, Trade.TOrderType.Buy, dd, price, qty);
                RateLimiterPenality += 3000;
                return success;
            }

            return false;
        }
        /// <summary>
        /// Place a 'fake' sell order
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="marketPrice"></param>
        /// <param name="ratio"></param>
        /// <param name="dt"></param>
        /// <param name="dry"></param>
        /// <returns></returns>
        public override bool Sell(AssetSymbol assetName, double marketPrice, double ratio, DateTime? dt, bool dry)
        {
            var dd = dt ?? DateTime.Now; 

            var bal = Balances(dd);
            var qty = bal[assetName.BaseName];
            if ( qty > 0)
            {
                NLog.LogManager.GetCurrentClassLogger().Info(dd + "|Place Order: Sell (" + assetName.SymbolName + ") - Quantity: " + (double)qty + " - Price:" + marketPrice + " - Total: " + (double)qty*marketPrice + " " + assetName.QuoteName );
                var success = PlaceOrder(assetName, Trade.TOrderType.Sell, dd, marketPrice, (double)qty);
                RateLimiterPenality += 3000;
                return success;
            }
            
            return false;
        }
        /// <summary>
        /// Place a 'fake' order on the 'fake' exchange market
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="orderType"></param>
        /// <param name="dt"></param>
        /// <param name="price"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        protected virtual bool PlaceOrder ( AssetSymbol assetName, Trade.TOrderType orderType, DateTime dt, double price, double amount )
        {
            var t = new Trade();
            t.Timestamp = dt;
            t.RefPrice = price;
            t.OrderType = orderType;
            t.Asset = assetName;

            var bal = Balances(dt);
            var balance = bal[assetName.BaseName];

            if ( orderType == Trade.TOrderType.Buy )
            {
                var fees = Fees((price*amount), orderType);
                decimal total = (decimal)((price*amount)-fees);

                t.Quantity = (double)amount;
                balance += (decimal)amount;
                _assetPortfolio[assetName.QuoteName] -= (double)total;
                //_money -= (double)total;
            }
            else
            {
                var fees = Fees((price*amount), orderType);
                decimal total = (decimal)((price*amount)-fees);

                t.Quantity = (double)amount;
                balance -= (decimal)amount;
                _assetPortfolio[assetName.QuoteName] += (double)total;
                //_money += (double)total;
            }

            
            _assetPortfolio[assetName.BaseName] = (double)balance;

            _tradeHistory.Add(t);

            return true;
        }
    }
}
