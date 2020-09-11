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
            var bot = new BotThune<ExchangeFake>();
            bot.MarketExchange.Deposit(500.0);
            var strategy = new Funiol();
            bot.AddStrategy(strategy, "XTZEUR", 0.5);
            bot.AddStrategy(strategy, "XRPEUR", 0.1);
            bot.AddStrategy(strategy, "BTCEUR", 0.4);
            bot.Sim();
            */
        }

        [Fact]
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

        [Fact]
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
