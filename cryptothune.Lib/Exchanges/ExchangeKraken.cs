using System;
using System.IO;
using System.Linq;
using CryptoExchange.Net.Authentication;
using Kraken.Net;
using Kraken.Net.Objects;
using Kraken.Net.Converters;
using CryptoExchange.Net.RateLimiter;
using CryptoExchange.Net.Objects;
using System.Data.SQLite;
using System.Collections.Generic;


namespace Cryptothune.Lib
{

    public class ExchangeKraken : IExchange
    {
        private KrakenClient kc = null;
        private double _krakenFeeSell = 0.26;
        private double _krakenFeeBuy = 0.18;

        private int _retryTimes = 4;
        private TimeSpan _retryDelay = TimeSpan.FromSeconds(1);

        public ExchangeKraken()
        {
            var cred = @"./credentials";
            if ( File.Exists(cred) )
            {
                using (var stream = File.OpenRead("./credentials"))
                {
                    var ko = new KrakenClientOptions();
                    ko.ApiCredentials = new ApiCredentials(stream, "krakenKey", "krakenSecret");
                    kc = new KrakenClient(ko);
                }
            }
            else
            {
                kc = new KrakenClient();    
            }

            kc.AddRateLimiter ( new RateLimiterAPIKey(15, TimeSpan.FromSeconds(3) ));

        }

        public virtual Dictionary<string, decimal> Balances()
        {
            var bal = RetryHelper<Dictionary<string, decimal>>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetBalances() );
            return bal.Data;
        }

        public virtual double Balance(string asset = "ZEUR")
        {
            var bal = RetryHelper<KrakenTradeBalance>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetTradeBalance(asset) );
            return (double)bal.Data.CombinedBalance;
        }

        public virtual double MarketPrice(string symbol)
        {
            var mk = RetryHelper<Dictionary<string, KrakenRestTick>>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetTickers(symbols: symbol) );
            return (double)mk.Data[symbol].LastTrade.Price;
        }

        public virtual KrakenTradesResult TradesHistory(string symbol, DateTime dt)
        {
            var l = RetryHelper<KrakenTradesResult>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetRecentTrades(symbol, dt) );
            return l.Data;
        }


        public virtual IEnumerable<double> PricesHistory(string symbol)
        {
            // To Do: Return here the Prices History on the specified symbol
            List<double> db = new List<double>();
            return db;
        }

        public virtual Trade LatestTrade(string symbol)
        {
            var mk = RetryHelper<KrakenUserTradesPage>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetTradeHistory() );
            var rt = mk.Data.Trades.First( x => x.Value.Symbol==symbol );
            
            var trade = new Trade();
            trade.RefPrice = (double)rt.Value.Price;
            trade.OrderType = rt.Value.Side==Kraken.Net.Objects.OrderSide.Buy?Trade.TOrderType.Buy:Trade.TOrderType.Sell;
            
            return trade;
        }

        /// <summary>
        /// Place a "Buy" order on the Kraken exchange market.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="price"></param>
        /// <param name="ratio"></param>
        /// <param name="dry"></param>
        /// <returns>true if the order was properly placed, false otherwise.</returns>
        public virtual bool Buy(string symbol, double price, double ratio, bool dry)
        {
            if ( ratio == 0)
            {
                return false;
            }

            var sym = RetryHelper<Dictionary<string, KrakenSymbol>>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetSymbols(symbols: symbol) );
            var fiatSymbol = sym.Data.First().Value.QuoteAsset;     // "XRPEUR" => "ZEUR"
            var totalBalance = Balance(fiatSymbol);                 // Total balance of the portfolio on this exchange market
            var qty = (totalBalance*ratio)/100.0;                   // Percentage of this total balance that can be used to place order.
            var bal = Balances();
            var availBalance = (double)bal[fiatSymbol];             // Available money for trading.
            if ( qty > availBalance)
            {
                qty = availBalance;
            }
/*
            var bal = Balances();
            var amount = bal["ZEUR"];
            var qty = (double)amount / price;
*/
            var order = RetryHelper<KrakenPlacedOrder>.RetryOnException(_retryTimes, _retryDelay, () => kc.PlaceOrder(symbol, OrderSide.Buy, OrderType.Market, quantity: (decimal)qty, validateOnly: dry) );
            return order.Success;
        }

        /// <summary>
        /// Place a "Sell" order on the Kraken exchange market.
        /// </summary>
        /// <param name="symbol">The crypto asset to sell for a given currency. ex: "BTCEUR" ></param>
        /// <param name="price">The wanted price (on the currency).</param>
        /// <param name="ratio">the pourcentage to qpply on the transaction.</param>
        /// <param name="dry">Is it for real or not?</param>
        /// <returns>true if the order was properly placed, false otherwise.</returns>
        public virtual bool Sell(string symbol, double price, double ratio, bool dry)
        {
            var sym = RetryHelper<Dictionary<string, KrakenSymbol>>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetSymbols(symbols: symbol) );
            var assetSymbol = sym.Data.First().Value.BaseAsset;
            var bal = Balances();
            //var assetSymbol = symbol.Substring(0, 3);   // "BTCEUR" => "BTC"
            //var qty = bal["XTZ"];
            var qty = bal[assetSymbol];
            if ( qty > 0)
            {
                var order = RetryHelper<KrakenPlacedOrder>.RetryOnException(_retryTimes, _retryDelay, () => kc.PlaceOrder(symbol, OrderSide.Sell, OrderType.Market, quantity: (decimal)qty, validateOnly: dry) );
                return order.Success;
            }
            
            return false;
        }
        
        public virtual string Name()
        {
            return "Kraken";
        }

        public virtual double Fees(double whole, Trade.TOrderType oType)
        {
            var fee = oType == Trade.TOrderType.Buy?_krakenFeeBuy:_krakenFeeSell;
            return (whole * fee) / 100;
        }

    }
}
