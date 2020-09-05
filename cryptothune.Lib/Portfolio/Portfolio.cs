using System;

namespace Cryptothune.Lib
{
    public class Portfolio : PortfolioBase
    {
        public Portfolio(IExchange exchange) : base(exchange)
        {
        }

        public double Balance()
        {
            var b = MarketExchange.GetBalances();
            return (double)b["ZEUR"];
        }

        public override void Export()
        {

        }

        public override bool Buy(string symbol, double marketPrice, bool dry=true )
        {
            MarketExchange.Buy(symbol, marketPrice, dry);
            return false;
        }


        public override bool Sell(string symbol, double marketPrice, bool dry=true )
        {
            MarketExchange.Sell(symbol, marketPrice, dry);
            return false;
        }

    }
}
