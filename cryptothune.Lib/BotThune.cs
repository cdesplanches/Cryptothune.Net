using System;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.Drawing;

namespace Cryptothune.Lib
{
    public class BotThune
    {
        public BotThune(PortfolioBase portfolio)
        {
            Portfolio = portfolio;
        }

        public PortfolioBase Portfolio { get; private set; }

        public void Sim(IStrategy strategy, string symbol = "XTZEUR")
        {
            var portfolio = (PortfolioFake)Portfolio;
            var prices = portfolio.ExportPrices(symbol).ToArray();

            var plt = new ScottPlot.Plot(2048, 1024);
            plt.PlotSignal(prices);

            var pltMoney = new ScottPlot.Plot(2048, 1024);


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
                        pltMoney.PlotPoint((double)index, portfolio.Money, color: Color.Black);  

                        refPrice = price;
                        portfolio.Sell(symbol, price, false);
                        prevAction = Trade.TOrderType.Sell; 
                    }
                    else
                    {
                        plt.PlotPoint((double)index, price, color: Color.Green );
                        pltMoney.PlotPoint((double)index, portfolio.Money, color: Color.Black);  

                        refPrice = price;
                        portfolio.Buy(symbol, price, false);
                        prevAction = Trade.TOrderType.Buy;
                    }
                }
                
                ++index;
            }

            plt.Title( symbol + " with " + strategy.Name() );
            plt.YLabel("Value (Euro))");
            plt.XLabel("Time" );
            plt.SaveFig(symbol + ".png" );

            pltMoney.Title( "Your Portfolio" );
            pltMoney.YLabel("Cash (Euro)) =" + portfolio.Money + " EUR & XTZ = " + portfolio.CyptoCurrency + " Nb Op = " + portfolio.NbOperations);
            pltMoney.XLabel("Time");
            pltMoney.SaveFig ("money.png");
        }

        public void Run(IStrategy strategy, string symbol = "XTZEUR")
        {
            while (true)
            {
                var marketPrice = Portfolio.MarketExchange.GetMarketPrice(symbol);
                var prevTrade = Portfolio.MarketExchange.GetLatestTrade(symbol);
                if ( strategy.Decide(marketPrice, prevTrade.RefPrice, prevTrade.OrderType) )
                {
                    if (prevTrade.OrderType == Trade.TOrderType.Buy)
                    {
                        Portfolio.Sell(symbol, marketPrice, false);
                    }
                    else
                    {
                        Portfolio.Buy(symbol, marketPrice, false);
                    }
                }

                Task.Delay(500);
            }
            
        }


        public async Task DryRun(IStrategy strategy, string symbol = "XTZEUR")
        {
            while (true)
            {
                var marketPrice = Portfolio.MarketExchange.GetMarketPrice(symbol);
                var prevTrade = Portfolio.MarketExchange.GetLatestTrade(symbol);
                if ( strategy.Decide(marketPrice, prevTrade.RefPrice, prevTrade.OrderType) )
                {
                    if (prevTrade.OrderType == Trade.TOrderType.Buy)
                    {
                        Portfolio.Sell(symbol, marketPrice);
                    }
                    else
                    {
                        Portfolio.Buy(symbol, marketPrice);
                    }
                }

                await Task.Delay(500);
            }
        }
    }
}
