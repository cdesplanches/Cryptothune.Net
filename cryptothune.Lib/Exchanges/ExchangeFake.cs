using System;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;

namespace Cryptothune.Lib
{
    public class ExchangeFake : ExchangeKraken
    {
        private Dictionary<string, decimal> _balances = new Dictionary<string, decimal>();
        private Dictionary<string, decimal> _balancesTrades = new Dictionary<string, decimal>();

        public ExchangeFake()
        {
            _balances.Add( "ZEUR", 0 );
        }

        private double _money { get; set; }


        public override Dictionary<string, decimal> Balances()
        {
            return _balances;
        }


        public override double Balance(string asset)
        {
            return _money;
        }

        public virtual double Deposit(double money)
        {
            _money += money;
            _balances["ZEUR"] += (decimal)money;
            return _money;
        }
        
        public override string Name()
        {
            return "Fake";
        }

        public override AssetName NormalizeSymbolName(string symbol)
        {
            var baseName = symbol.Substring(0, 3);
            var quote = "Z" + symbol.Substring(symbol.Length-3, 3);

            _balances[symbol] = 0;
            return new AssetName(symbol, baseName, quote);
        }

        public override IEnumerable<double> PricesHistory(AssetName assetName)
        {
            var con = ExportTradesOnDB(assetName);

            // Get all prices
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = @"SELECT price FROM history";
            cmd.ExecuteNonQuery();

            SQLiteDataReader rdr = cmd.ExecuteReader();
            var prc = rdr.Cast<IDataRecord>().Select(r => (double)r["price"]).ToArray();
            return prc;
        }


        public SQLiteConnection ExportTradesOnDB(AssetName assetName)
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

                PreventRateLimit(); // To avoid a rate limit exception on Kraken API public calls.
//                Thread.Sleep(3000); 
            }

            return con;
        }


        public override bool Buy(AssetName assetName, double marketPrice, double ratio, bool dry)
        {
            var totalBalance = Balance(assetName.QuoteName);
            var qty = (totalBalance*ratio)/100.0;
            var fees = Fees(qty, Trade.TOrderType.Buy);
            var real = (qty + fees);
            _balances[assetName.SymbolName] = (decimal)(real/marketPrice);
            _balancesTrades[assetName.SymbolName] = (decimal)marketPrice;
            _balances[assetName.QuoteName] -= (decimal)real;
            if (_balances[assetName.QuoteName] < 0)
                _balances[assetName.QuoteName] = 0;

            _money -= fees;
            NLog.LogManager.GetCurrentClassLogger().Info("Buy");
            return true;
        }


        public override bool Sell(AssetName assetName, double marketPrice, double ratio, bool dry)
        {
            if ( !_balancesTrades.ContainsKey(assetName.SymbolName) )
            {
                return false;
            }
            var totalBalance = Balance(assetName.QuoteName);
            var qty = (double)_balances[assetName.SymbolName]*marketPrice;
            var prevqty = _balances[assetName.SymbolName]*_balancesTrades[assetName.SymbolName];
            var fees = Fees(qty, Trade.TOrderType.Sell);
            var real = (qty - fees);
           
            _balances[assetName.SymbolName] = 0;
            _balancesTrades[assetName.SymbolName] = (decimal)marketPrice;
            _balances[assetName.QuoteName] += (decimal)real;
            
            var gain = real-(double)prevqty;
            _money += gain;

            NLog.LogManager.GetCurrentClassLogger().Info("Sell");
            return true;
        }
    }
}
