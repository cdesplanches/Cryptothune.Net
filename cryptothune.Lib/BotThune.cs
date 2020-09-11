using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;

namespace Cryptothune.Lib
{
    public class StrategyTuple
    {
        public StrategyTuple(IStrategy strategy, double percent)
        {
            Strategy = strategy;
            Percent = percent;
        }
        public IStrategy    Strategy { get; private set; }
        public double       Percent { get; private set; }
    }

    public class BotThune<T> where T : IExchange, new()
    {
        private Dictionary<string, StrategyTuple> _strategies = new Dictionary<string, StrategyTuple>();

        public BotThune()
        {
            MarketExchange = new T();
        }

        public T MarketExchange { get; protected set; }

        /// <summary>
        /// Add a trading strategy to a specified symbol.
        /// </summary>
        /// <param name="strategy">the strategy to apply on the selected symbol. <see cref="IStrategy"/> </param>
        /// <param name="symbol">Symbol ex: "XRPEUR" </param>
        /// <param name="percent">Percentage of the portfolio to allocate to this strategy</param>
        public void AddStrategy( IStrategy strategy, string symbol, double percent = 100.0)
        {
            _strategies.Add(symbol, new StrategyTuple(strategy, percent));
        }

        /// <summary>
        /// Launch a simulation of trades on the current exchange.
        /// It will:
        ///     1. Create or update the complete trading history on all symbols that strategies are responsible of
        ///     2. Execute the strategy of Buy/Sell, starting from the beginnig of the symbol history life
        ///     3. Export a picture of the Buy/Sell history (graph)
        ///     4. Export a picture of the evolution of the fiat balance.
        /// </summary>
        public void Sim()
        {
            var pltMoney = new ScottPlot.Plot(2048, 1024);
            int cptOp = 0;
           
            foreach ( var stratDef in _strategies )
            {
                string symbol = stratDef.Key;
                IStrategy strategy = stratDef.Value.Strategy;
                var prices = MarketExchange.PricesHistory(symbol).ToArray();

                var plt = new ScottPlot.Plot(2048, 1024);
                plt.PlotSignal(prices);


                int index = 0;
                var prevAction = Trade.TOrderType.Sell;
                double refPrice = 0.0;
              
                foreach (var price in prices)
                {
                    if ( strategy.Decide(price, refPrice, prevAction) && (index>10) )    // Skip the first 10 transactions
                    {
                        if (prevAction == Trade.TOrderType.Buy)
                        {
                            plt.PlotPoint((double)index, price, color: Color.Red );
                            pltMoney.PlotPoint((double)index, MarketExchange.Balance("ZEUR"), color: Color.Black);  

                            refPrice = price;
                            MarketExchange.Sell(symbol, price, stratDef.Value.Percent, false);
                            ++cptOp;
                            prevAction = Trade.TOrderType.Sell; 
                        }
                        else
                        {
                            plt.PlotPoint((double)index, price, color: Color.Green );
                            pltMoney.PlotPoint((double)index, MarketExchange.Balance("ZEUR"), color: Color.Black);  

                            refPrice = price;
                            MarketExchange.Buy(symbol, price, stratDef.Value.Percent, false);
                            ++cptOp;
                            prevAction = Trade.TOrderType.Buy;
                        }
                    }
                    
                    ++index;
                }

                plt.Title( symbol + " with " + strategy.Name() );
                plt.YLabel("Value (Euro))");
                plt.XLabel("Time" );
                plt.SaveFig(symbol + ".png" );
            }

            pltMoney.Title( "Your Portfolio" );
            pltMoney.YLabel("Cash (Euro)) =" + MarketExchange.Balance("ZEUR") + " EUR & Nb Op = " + cptOp);
            pltMoney.XLabel("Time");
            pltMoney.SaveFig ("money.png");
        }

        public void Run()
        {
            while (true)
            {
                foreach ( var stratDef in _strategies )
                {
                    string symbol = stratDef.Key;
                    IStrategy strategy = stratDef.Value.Strategy;

                    var marketPrice = MarketExchange.MarketPrice(symbol);
                    var prevTrade = MarketExchange.LatestTrade(symbol);
                    if ( strategy.Decide(marketPrice, prevTrade.RefPrice, prevTrade.OrderType) )
                    {
                        if (prevTrade.OrderType == Trade.TOrderType.Buy)
                        {
                            MarketExchange.Sell(symbol, marketPrice, stratDef.Value.Percent, false);
                        }
                        else
                        {
                            MarketExchange.Buy(symbol, marketPrice, stratDef.Value.Percent, false);
                        }
                    }

                    Task.Delay(18000).Wait();
                }
            }
            
        }


        public void DryRun()
        {
            while (true)
            {
                foreach ( var stratDef in _strategies )
                {
                    string symbol = stratDef.Key;
                    IStrategy strategy = stratDef.Value.Strategy;

                    var marketPrice = MarketExchange.MarketPrice(symbol);
                    var prevTrade = MarketExchange.LatestTrade(symbol);
                    if ( strategy.Decide(marketPrice, prevTrade.RefPrice, prevTrade.OrderType) )
                    {
                        if (prevTrade.OrderType == Trade.TOrderType.Buy)
                        {
                            MarketExchange.Sell(symbol, marketPrice, stratDef.Value.Percent, true);
                        }
                        else
                        {
                            MarketExchange.Buy(symbol, marketPrice, stratDef.Value.Percent, true);
                        }
                    }

                    Task.Delay(18000).Wait();
                }
            }
        }
    }
}
