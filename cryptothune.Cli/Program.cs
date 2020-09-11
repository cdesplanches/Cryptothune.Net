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
                       }


                       if ( o.Simulate )
                       {
                            var bot = new BotThune<ExchangeFake>();
                            bot.MarketExchange.Deposit(500.0);
                            var strategy = new Funiol();
                            bot.AddStrategy(strategy, "XTZEUR", 70.0 );
                            bot.AddStrategy(strategy, "XRPEUR", 30.0 );
                            bot.Sim();
                       }
                       else
                       {
                            var bot = new BotThune<ExchangeKraken>();
                            var strategy = new Funiol();
                            bot.AddStrategy(strategy, "XTZEUR", 50.0);
                            bot.AddStrategy(strategy, "BTCEUR", 45.0);
                            bot.AddStrategy(strategy, "XRPEUR", 5.0 );
                            if ( o.DryRun )
                            {
                                bot.DryRun();
                            }
                            else    // Oh; real run
                            {
                                bot.Run();
                            }
                            
                       }


                   });


                   return 0;

        }
    }
}
