using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;

namespace Cryptothune.Lib
{
    /// <summary>
    /// The Bot class that apply strategy on a given crypto market exchange place
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BotThune<T> where T : IExchange, new()
    {
        private List<StrategyObject> _strategies = new List<StrategyObject>();
        /// <summary>
        /// ctor
        /// </summary>
        public BotThune()
        {
            MarketExchange = new T();
        }
        /// <summary>
        /// return the market exchange place object
        /// </summary>
        /// <value>the market exchange place</value>
        public T MarketExchange { get; protected set; }

        /// <summary>
        /// Add a trading strategy to a specified symbol.
        /// </summary>
        /// <param name="strategy">the strategy to apply on the selected symbol. <see cref="IStrategy"/> </param>
        /// <param name="symbol">Symbol ex: "XRPEUR" </param>
        /// <param name="percent">Percentage of the portfolio to allocate to this strategy</param>
        public void AddStrategy( IStrategy strategy, string symbol, double percent = 100.0)
        {
            var assetName = MarketExchange.NormalizeSymbolName(symbol);
            _strategies.Add( new StrategyObject(strategy, assetName, percent) );
        }

        /// <summary>
        /// Launch a simulation of trades on the current exchange.
        /// It will:
        ///     1. Create or update the complete trading history on all symbols that strategies are responsible of
        ///     2. Execute the strategy of Buy/Sell, starting from the beginnig of the symbol history life
        ///     3. Export a picture of the Buy/Sell history (graph)
        ///     4. Export a picture of the evolution of the fiat balance.
        /// </summary>
        public void Sim( DateTime? startDate = null, DateTime? endDate = null )
        {
            NLog.LogManager.GetCurrentClassLogger().Info("Start simulate Buy/Sell trades");

            var marketPrices = new List<ValueTuple<double, string, double>>();
            var plt = new Dictionary<string, ScottPlot.Plot>();
            foreach ( var stratDef in _strategies )
            {
                var symbol = stratDef.AssetName;
                var strategy = stratDef.Strategy;
                var prices = MarketExchange.PricesHistory(symbol);

                var plot = new ScottPlot.Plot(2048, 1024);
                plot.Ticks(dateTimeX: true);
                plot.PlotSignalXY(prices.Keys.ToArray(), prices.Values.ToArray());
                plot.Title( symbol.SymbolName + " with " + strategy.Name() );
                plot.YLabel("Value (Euro))");
                plot.XLabel("Time" );
                plt.Add(symbol.SymbolName, plot);

                foreach ( var price in prices )
                {
                    marketPrices.Add( new ValueTuple<double, string, double>(price.Key, symbol.BaseName, price.Value) );
                }
            }
            marketPrices.Sort();
           

            var pltMoney = new ScottPlot.Plot(2048, 1024);
            pltMoney.Ticks(dateTimeX: true);
            int cptOp = 0;           
            
            var dt = startDate ?? DateTime.FromOADate(marketPrices.First().Item1);
            var edt = endDate ?? DateTime.Now;
            var ms = 0;//Math.Max(0, (dt-sdt).TotalMilliseconds);               // 0 or the number of millisecond betzeen startdate and the first datetime entry on the list of prices

            var eoa = edt.ToOADate();
            List<double> xs = new List<double>();
            List<double> ys = new List<double>();
            foreach ( var marketEntry in marketPrices )
            {
                var oa = dt.AddMilliseconds(ms).ToOADate();
                if ( oa >= eoa)
                    break;

                if (marketEntry.Item1>=oa)
                {
                    dt = DateTime.FromOADate(marketEntry.Item1);
                    foreach ( var stratDef in _strategies )
                    {
                        var assetName = stratDef.AssetName;
                        if ( assetName.BaseName == marketEntry.Item2 )
                        {
                            var strategy = stratDef.Strategy;
                            var marketPrice = marketEntry.Item3;
                            var prevTrade = MarketExchange.LatestTrade(assetName);
                            if ( strategy.Decide(marketEntry.Item3, prevTrade.RefPrice, prevTrade.OrderType) )
                            {
                                if (prevTrade.OrderType == Trade.TOrderType.Buy)
                                {
                                    if ( MarketExchange.Sell(assetName, marketPrice, stratDef.Percentage, dt, false) )
                                    {
                                        ++cptOp;
                                        plt[assetName.SymbolName].PlotPoint(marketEntry.Item1, marketPrice, color: Color.Red );
                                        xs.Add(oa);
                                        ys.Add(MarketExchange.Balance("ZEUR"));
                                        //pltMoney.PlotPoint(oa, MarketExchange.Balance("ZEUR"), color: Color.Green );
                                    }
                                        
                                }
                                else
                                {
                                    if ( MarketExchange.Buy(assetName, marketPrice, stratDef.Percentage, dt, false) )
                                    {
                                        ++cptOp;
                                        plt[assetName.SymbolName].PlotPoint(marketEntry.Item1, marketPrice, color: Color.Green );
                                        xs.Add(oa);
                                        ys.Add(MarketExchange.Balance("ZEUR"));
                                        //pltMoney.PlotPoint(oa, MarketExchange.Balance("ZEUR"), color: Color.Red );
                                    }
                                }
                            }
                        }
                    }
                }

                ms = MarketExchange.PreventRateLimit();
            }

            pltMoney.PlotSignalXY( xs.ToArray(), ys.ToArray(), color: Color.Red );
            pltMoney.Title( "Your Portfolio" );
            pltMoney.YLabel("Cash (Euro)) =" + MarketExchange.Balance("ZEUR") + " EUR & Nb Op = " + cptOp);
            pltMoney.XLabel("Time");
            pltMoney.SaveFig ("money.png");

            foreach ( var ppp in plt )
            {
                ppp.Value.SaveFig(ppp.Key + ".png" );
            }
        }
        /// <summary>
        /// Perform a real run
        /// </summary>
        public void Run()
        {
            while (true)
            {
                foreach ( var stratDef in _strategies )
                {
                    var assetName = stratDef.AssetName;
                    var strategy = stratDef.Strategy;

                    var marketPrice = MarketExchange.MarketPrice(assetName);
                    var prevTrade = MarketExchange.LatestTrade(assetName);
                    if ( strategy.Decide(marketPrice, prevTrade.RefPrice, prevTrade.OrderType) )
                    {
                        if (prevTrade.OrderType == Trade.TOrderType.Buy)
                        {
                            MarketExchange.Sell(assetName, marketPrice, stratDef.Percentage, null, false);
                        }
                        else
                        {
                            MarketExchange.Buy(assetName, marketPrice, stratDef.Percentage, null, false);
                        }
                    }

                    MarketExchange.PreventRateLimit();
                }
            }
            
        }
        /// <summary>
        /// Perform a dry run
        /// </summary>
        public void DryRun()
        {
            while (true)
            {
                foreach ( var stratDef in _strategies )
                {
                    var assetName = stratDef.AssetName;
                    var strategy = stratDef.Strategy;

                    var marketPrice = MarketExchange.MarketPrice(assetName);
                    var prevTrade = MarketExchange.LatestTrade(assetName);
                    if ( strategy.Decide(marketPrice, prevTrade.RefPrice, prevTrade.OrderType) )
                    {
                        if (prevTrade.OrderType == Trade.TOrderType.Buy)
                        {
                            MarketExchange.Sell(assetName, marketPrice, stratDef.Percentage, null, true);
                        }
                        else
                        {
                            MarketExchange.Buy(assetName, marketPrice, stratDef.Percentage, null, true);
                        }
                    }

                    MarketExchange.PreventRateLimit();
                }
            }
        }
    }
}
