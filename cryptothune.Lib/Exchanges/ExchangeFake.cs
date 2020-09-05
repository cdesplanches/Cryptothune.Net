using System.Linq;
using System.Data;
using System.Data.SQLite;
using System.Collections.Generic;

namespace Cryptothune.Lib
{
    public class ExchangeFake : ExchangeKraken
    {
        private double _krakenFee = 0.26;

        public ExchangeFake()
        {
        }

        public override Dictionary<string, decimal> GetBalances()
        {
            var bal = base.GetBalances();
            return bal;
        }
        
        public override string Name()
        {
            return "Fake";
        }

        public override double Fees(double whole)
        {
            return (whole * _krakenFee) / 100;
        }


        public virtual IEnumerable<double> GetPricesHistory(string symbol)
        {
            var con = base.ExportTradesOnDB(symbol);

            // Get all prices
            var cmd = new SQLiteCommand(con);
            cmd.CommandText = @"SELECT price FROM history";
            cmd.ExecuteNonQuery();

            SQLiteDataReader rdr = cmd.ExecuteReader();
            var prc = rdr.Cast<IDataRecord>().Select(r => (double)r["price"]).ToArray();
            return prc;
        }
    }
}
