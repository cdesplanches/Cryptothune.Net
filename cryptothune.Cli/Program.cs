using System;
using CommandLine;
using Cryptothune.Lib;

namespace Cryptothune.Cli
{
    class Program
    {
        public class Options
        {
            [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
            public bool Verbose { get; set; }

            [Option('s', "sim", Required = false, HelpText = "Launch a simulation.")]
            public bool Simulate { get; set; }

            [Option('d', "dry", Default = false, Required = false, HelpText = "Run.")]
            public bool DryRun { get; set; }
        }

        static int Main(string[] args)
        {
            /*
            var exchange = new ExchangeKraken();
            var portfolio = new Portfolio(exchange);
            var bot = new BotThune(portfolio);
            bot.Run(new Funiol(), "XTZEUR" );

            return 0;
*/


             Parser.Default.ParseArguments<Options>(args)
                   .WithParsed<Options>(o =>
                   {
                       if (o.Verbose)
                       {
                           Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                       }
                       else
                       {
                           Console.WriteLine($"Current Arguments: -v {o.Verbose}");
                           Console.WriteLine("Quick Start Example!");
                       }


                       if ( o.Simulate )
                       {
                            var exchange = new ExchangeFake();
                            var portfolio = new PortfolioFake(exchange);
                            portfolio.Money = 500.0;    // Put 500 EUR as a gift.
                            var bot = new BotThune(portfolio);
                            bot.Sim(new Funiol(), "XTZEUR");
                       }
                       else
                       {
                            var exchange = new ExchangeKraken();
                            var portfolio = new Portfolio(exchange);
                            var bot = new BotThune(portfolio);
                            if ( o.DryRun )
                            {
                                bot.DryRun(new Funiol(), "XTZEUR");
                            }
                            else    // Oh; real run
                            {
                                bot.Run(new Funiol(), "XTZEUR");
                            }
                            
                       }


                   });


                   return 0;

        }
    }
}
