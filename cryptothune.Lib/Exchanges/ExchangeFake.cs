using System;
using System.Linq;
using System.Data;
using System.Data.SQLite;
using System.Threading;
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
            _balances.Add( "XRPEUR", 0 );
            _balances.Add( "XTZEUR", 0 );
            _balances.Add( "BTCEUR", 0 );
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

        public override IEnumerable<double> PricesHistory(string symbol)
        {
            var con = ExportTradesOnDB(symbol);

            // Get all prices
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = @"SELECT price FROM history";
            cmd.ExecuteNonQuery();

            SQLiteDataReader rdr = cmd.ExecuteReader();
            var prc = rdr.Cast<IDataRecord>().Select(r => (double)r["price"]).ToArray();
            return prc;
        }


        public SQLiteConnection ExportTradesOnDB(string symbol)
        {
            string cs = @"URI=file:" + symbol + ".db";
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
                var trades = TradesHistory(symbol, dt);
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

                Thread.Sleep(3000); // To avoid a rate limit exception on Kraken API public calls.
            }

            return con;
        }


        public override bool Buy(string symbol, double marketPrice, double ratio, bool dry)
        {
            var totalBalance = Balance("ZEUR");
            var qty = (totalBalance*ratio)/100.0;
            var fees = Fees(qty, Trade.TOrderType.Buy);
            var real = (qty + fees);
            _balances[symbol] = (decimal)(real/marketPrice);
            _balancesTrades[symbol] = (decimal)marketPrice;
            _balances["ZEUR"] -= (decimal)real;
            if (_balances["ZEUR"] < 0)
                _balances["ZEUR"] = 0;

            _money -= fees;
            return true;
        }


        public override bool Sell(string symbol, double marketPrice, double ratio, bool dry)
        {
            var totalBalance = Balance("ZEUR");
            var qty = (double)_balances[symbol]*marketPrice;
            var prevqty = _balances[symbol]*_balancesTrades[symbol];
            var fees = Fees(qty, Trade.TOrderType.Sell);
            var real = (qty - fees);
           
            _balances[symbol] = 0;
            _balancesTrades[symbol] = (decimal)marketPrice;
            _balances["ZEUR"] += (decimal)real;
            
            var gain = real-(double)prevqty;
            _money += gain;

            return true;
        }
    }
}
