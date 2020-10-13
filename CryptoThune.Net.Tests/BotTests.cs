using System;
using NUnit.Framework;
using CryptoThune.Net;

namespace CryptoThune.Net.Tests
{
    [TestFixture]
    public class BotTests
    {
        [TestCase()]
        public void TestStrategies()
        {
            var bot = new BotThune<ExchangeFake>();
            var st = bot.Strategies;
            Assert.IsEmpty(st, "looks like you have Strategy(ies) available on a brand new bot?!");
        }

        [TestCase()]
        public void TestAssets()
        {
            //var bot = new BotThune<ExchangeKraken>();

        }
        
        [TestCase()]
        public void TestSim()
        {
            /*
            var bot = new BotThune<ExchangeFake>();
            bot.MarketExchange.Deposit(500.0);
            var strategy = new Funiol();
            bot.AddStrategy(strategy, "XTZEUR", 0.5);
            bot.AddStrategy(strategy, "XRPEUR", 0.1);
            bot.AddStrategy(strategy, "BTCEUR", 0.4);
            bot.Sim();
            */
        }

        [TestCase()]
        public void TestDryRun()
        {
            /*
            var bot = new BotThune<ExchangeKraken>();
            var strategy = new Funiol();
            bot.AddStrategy(strategy, "XTZEUR", 0.5);
            bot.AddStrategy(strategy, "XRPEUR", 0.1);
            bot.AddStrategy(strategy, "BTCEUR", 0.4);
            bot.DryRun();
            */
        }

        [TestCase()]
        public void TestRun()
        {
            /*
            var bot = new BotThune<ExchangeKraken>();
            var strategy = new Funiol();
            bot.AddStrategy(strategy, "XTZEUR", 0.5);
            bot.AddStrategy(strategy, "XRPEUR", 0.1);
            bot.AddStrategy(strategy, "BTCEUR", 0.4);
            bot.Run();
            */
        }
    }
}
