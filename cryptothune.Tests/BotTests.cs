using System;
using Xunit;
using Cryptothune.Lib;

namespace Cryptothune.Tests
{
    public class BotTests
    {
        [Fact]
        public void TestAssets()
        {
            //var bot = new BotThune<ExchangeKraken>();

        }
        
        [Fact]
        public void TestSim()
        {
            /*
            var exchange = new ExchangeFake();
            var portfolio = new PortfolioFake(exchange);
            portfolio.Money = 500.0;    // Put 500 EUR as a gift.
            var bot = new BotThune(portfolio);
            bot.Sim(new Funiol(), "XTZEUR");
            */
        }

        [Fact]
        public void TestDryRun()
        {
            /*
            var exchange = new ExchangeKraken();
            var portfolio = new Portfolio(exchange);
            var bot = new BotThune(portfolio);
            bot.Run(new Funiol(), "XTZEUR" );
            */
        }

        [Fact]
        public void TestRun()
        {
            /*
            var exchange = new ExchangeKraken();
            var portfolio = new Portfolio(exchange);
            var bot = new BotThune(portfolio);
            bot.Run(new Funiol(), "XTZEUR" );
            */
        }
    }
}