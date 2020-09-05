using System;
using System.Threading.Tasks;
using Cryptothune.Lib;

namespace Cryptothune.Cli
{
    class Program
    {
        static int Main(string[] args)
        {
            var exchange = new ExchangeKraken();
            var portfolio = new Portfolio(exchange);
            var bot = new BotThune(portfolio);
            bot.Run(new Funiol(), "XTZEUR" );

            return 0;
        }
    }
}
