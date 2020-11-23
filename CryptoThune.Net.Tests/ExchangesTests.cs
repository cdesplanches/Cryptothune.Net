using System;
using NUnit.Framework;
using CryptoThune.Net;

namespace CryptoThune.Net.Tests
{
    [TestFixture]
    public class ExchangesTests
    {
        /// <summary>
        /// Test to get latest trades
        /// </summary>
        [TestCase()]
        public void TestLatestTrade()
        {
            ExchangeKraken kr = new ExchangeKraken();
            var xtz = kr.NormalizeSymbolName("XRPEUR");
            var tr = kr.LatestTrade(xtz);

            var xrp = kr.NormalizeSymbolName("XRPEUR");
            tr = kr.LatestTrade(xrp);

            var btc = kr.NormalizeSymbolName("BTCEUR");
            tr = kr.LatestTrade(btc);
        }
        [TestCase()]
        public void TestRecentTrade()
        {
            ExchangeKraken kr = new ExchangeKraken();
            var xtz = kr.NormalizeSymbolName("BTCEUR");
            var tr = kr.TradesHistory(xtz, DateTime.Now.AddMonths(-2));
        }
        /// <summary>
        /// Test the creation of Symbol Asset Object.
        /// </summary>
        [TestCase()]
        public void TestNormalizeSymbolName()
        {
            ExchangeFake kr = new ExchangeFake();
            var xtz = kr.NormalizeSymbolName("XRPEUR");
            Assert.AreEqual(xtz.SymbolName, "XRPEUR");
        }
        [TestCase()]
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

        [TestCase()]
        public void TestFakeBuy()
        {
            ExchangeFake kr = new ExchangeFake();
            var xtz = kr.NormalizeSymbolName("XTZEUR");
            kr.Deposit(500.0);
            kr.Buy(xtz, 2.19, 50, null, true);
        }

        [TestCase()]
        public void TestFakeSell()
        {
            ExchangeFake kr = new ExchangeFake();
            var xtz = kr.NormalizeSymbolName("XTZEUR");
            kr.Deposit(500.0);
            kr.Sell(xtz, 2.35, 50, null, true);
        }

        [TestCase()]
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

        [TestCase()]
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
