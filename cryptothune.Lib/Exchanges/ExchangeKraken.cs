using System;
using System.IO;
using System.Linq;
using System.Threading;
using CryptoExchange.Net.Authentication;
using Kraken.Net;
using System.Data.SQLite;
using System.Collections.Generic;

namespace Cryptothune.Lib
{
    public class ExchangeKraken : IExchange
    {
        private KrakenClient kc = null;
        private double _krakenFee = 0.26;

        public ExchangeKraken()
        {
            var cred = @"./credentials";
            if ( File.Exists(cred) )
            {
                using (var stream = File.OpenRead("./credentials"))
                {
                    var ko = new KrakenClientOptions();
                    ko.ApiCredentials = new ApiCredentials(stream, "krakenKey", "krakenSecret");
                    kc = new KrakenClient(ko);
                }
            }
            else
            {
                kc = new KrakenClient();    
            }
        }

        public virtual Dictionary<string, decimal> GetBalances()
        {
            var bal = kc.GetBalances();
            return bal.Data;
        }

        public double GetMarketPrice(string symbol)
        {
            var mk = kc.GetTickers(symbols: symbol);
            return (double)mk.Data[symbol].LastTrade.Price;
        }

        public Trade GetLatestTrade(string symbol)
        {
            var mk = kc.GetTradeHistory();
            var rt = mk.Data.Trades.First( x => x.Value.Symbol==symbol );
            
            var trade = new Trade();
            trade.RefPrice = (double)rt.Value.Price;
            trade.OrderType = rt.Value.Side==Kraken.Net.Objects.OrderSide.Buy?Trade.TOrderType.Buy:Trade.TOrderType.Sell;
            
            return trade;
        }

        public virtual void Buy(string symbol, double price, bool dry)
        {
            var bal = GetBalances();
            var amount = bal["ZEUR"];
            var qty = (double)amount / price;
            kc.PlaceOrder(symbol, Kraken.Net.Objects.OrderSide.Buy, Kraken.Net.Objects.OrderType.Market, quantity: (decimal)qty, validateOnly: true);
        }

        public virtual void Sell(string symbol, double price, bool dry)
        {
            var bal = GetBalances();
            var qty = bal["XTZ"];
            kc.PlaceOrder(symbol, Kraken.Net.Objects.OrderSide.Sell, Kraken.Net.Objects.OrderType.Market, quantity: qty, validateOnly: true);
        }
        
        public virtual string Name()
        {
            return "Kraken";
        }

        public virtual double Fees(double whole)
        {
            return (whole * _krakenFee) / 100;
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
                var l = kc.GetRecentTrades(symbol, dt);
                using (var transaction = con.BeginTransaction())
                {
                    foreach (var it in l.Data.Data)
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


                dt = l.Data.Last;
                cmd.CommandText = @"UPDATE lastcheck SET timestamp = '" + dt.Ticks + "'";
                cmd.ExecuteNonQuery();


                if (l.Data.Data.Count() < 1000)
                {
                    fullyUpdated = true;
                }

                Thread.Sleep(3000); // To avoid a rate limit exception on Kraken API public calls.
            }

            return con;
        }
    }
}
