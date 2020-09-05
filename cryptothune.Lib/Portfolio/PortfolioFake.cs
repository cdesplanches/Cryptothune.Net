using System;
using System.Linq;
using System.Collections.Generic;
using System.Drawing;

namespace Cryptothune.Lib
{
    public class PortfolioFake : PortfolioBase
    {
        public PortfolioFake(ExchangeFake exchange) : base (exchange)
        {
            Money = 0.0;
            CyptoCurrency = 0.0;
            NbOperations = 0;
        }



        public double CyptoCurrency { get; set; }
        public double Money { get; set; }
        public int NbOperations { get; private set; }


        public IEnumerable<double> ExportPrices(string symbol)
        {
            var exchFake = (ExchangeFake)(MarketExchange);
            // Export a complete history of the price values for q given symbol
            var prices = exchFake.GetPricesHistory(symbol).ToArray();

            return prices;
        }

        public void Report()
        {
            
        }

        public double Balance()
        {
            return Money;
        }

        public override bool Buy(string symbol, double marketPrice, bool dry)
        {
            CyptoCurrency = (Money/marketPrice);
            Money = 0;
            NbOperations++;

            return true;
        }


        public override bool Sell(string symbol, double marketPrice, bool dry)
        {
            double qty = CyptoCurrency*marketPrice;
            var fees = MarketExchange.Fees(qty);
            Money = (qty - fees);
            CyptoCurrency = 0;
            NbOperations++;

            return true;
        }

    }
}
