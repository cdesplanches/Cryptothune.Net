using System;
using Xunit;
using Cryptothune.Lib;

namespace Cryptothune.Tests
{
    public class ExchangesTests
    {
        [Fact]
        public void TestFakeBuySell()
        {
            ExchangeFake kr = new ExchangeFake();
            kr.Deposit(500.0);
            kr.Buy("XTZEUR", 2.19, 70, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );
            kr.Buy("XRPEUR", 0.19, 10, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );
            kr.Buy("BTCEUR", 8500, 20, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );           // Should be 500.0 = Everything is invested
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );        // Should be 0 = nothing left to be invested
            
            
            
            kr.Sell("XTZEUR", 2.50, 70, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );
            kr.Sell("XRPEUR", 0.08, 10, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );
            kr.Sell("BTCEUR", 8450, 20, true);
            Console.WriteLine ( "Total: " + kr.Balance("ZEUR") );           
            Console.WriteLine ( "Dispo: " + kr.Balances()["ZEUR"] );
        }

        [Fact]
        public void TestFakeBuy()
        {
            ExchangeFake kr = new ExchangeFake();
            kr.Deposit(500.0);
            kr.Buy("XTZEUR", 2.19, 50, true);
        }

        [Fact]
        public void TestFakeSell()
        {
            ExchangeFake kr = new ExchangeFake();
            kr.Deposit(500.0);
            kr.Sell("XTZEUR", 2.35, 50, true);
        }

        [Fact]
        public void TestBuy()
        {
            ExchangeKraken kr = new ExchangeKraken();
            kr.Buy("XTZEUR", 2.13, 50, true);
        }

        [Fact]
        public void TestSell()
        {
            ExchangeKraken kr = new ExchangeKraken();
            kr.Sell("BTCEUR", 2.18, 50, true);
        }
    }
}
