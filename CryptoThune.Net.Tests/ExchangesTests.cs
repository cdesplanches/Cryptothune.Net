using System;
using Xunit;
using CryptoThune.Net;

namespace Cryptothune.Tests
{
    public class ExchangesTests
    {
        [Fact]
        public void TestFakeBuySell()
        {
            ExchangeFake kr = new ExchangeFake();
            var xtz = kr.NormalizeSymbolName("XTZEUR");
            var btc = kr.NormalizeSymbolName("BTCEUR");
            var xrp = kr.NormalizeSymbolName("XRPEUR");
            kr.Deposit(500.0);

            // Buy
            kr.Buy(xtz, 2.19, 70, null, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );
            kr.Buy(xrp, 0.19, 10, null, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );
            kr.Buy(btc, 8500, 20, null, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );           // Should be 500.0 = Everything is invested
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );        // Should be 0 = nothing left to be invested
            
            
            // Sell
            kr.Sell(xtz, 2.50, 70, null, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );
            kr.Sell(xrp, 0.08, 10, null, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );
            kr.Sell(btc, 8450, 20, null, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );           
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );
        }

        [Fact]
        public void TestFakeBuy()
        {
            ExchangeFake kr = new ExchangeFake();
            var xtz = kr.NormalizeSymbolName("XTZEUR");
            kr.Deposit(500.0);
            kr.Buy(xtz, 2.19, 50, null, true);
        }

        [Fact]
        public void TestFakeSell()
        {
            ExchangeFake kr = new ExchangeFake();
            var xtz = kr.NormalizeSymbolName("XTZEUR");
            kr.Deposit(500.0);
            kr.Sell(xtz, 2.35, 50, null, true);
        }

        [Fact]
        public void TestBuy()
        {
            ExchangeKraken kr = new ExchangeKraken();
            var xtz = kr.NormalizeSymbolName("XTZEUR");
            var btc = kr.NormalizeSymbolName("BTCEUR");
            var xrp = kr.NormalizeSymbolName("XRPEUR");

            kr.Buy(xtz, 2.13, 50, null, true);
            kr.Buy(btc, 8800, 40, null, true);
            kr.Buy(xrp, 0.20, 10, null, true);
        }

        [Fact]
        public void TestSell()
        {
            ExchangeKraken kr = new ExchangeKraken();
            var xtz = kr.NormalizeSymbolName("XTZEUR");
            var btc = kr.NormalizeSymbolName("BTCEUR");
            var xrp = kr.NormalizeSymbolName("XRPEUR");

            kr.Sell(xtz, 2.18, 50, null, true);
            kr.Sell(btc, 10000, 40, null, true);
            kr.Sell(xrp, 0.40, 10, null, true);
        }
    }
}
