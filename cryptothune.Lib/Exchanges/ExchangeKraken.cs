using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CryptoExchange.Net.Authentication;
using Kraken.Net;
using Kraken.Net.Objects;
using Kraken.Net.Converters;
using CryptoExchange.Net.RateLimiter;
using System.Collections.Generic;
using NLog;



namespace Cryptothune.Lib
{

    public class ExchangeKraken : IExchange
    {
        private KrakenClient kc = null;
        private double _krakenFeeSell = 0.26;
        private double _krakenFeeBuy = 0.18;

        private int _retryTimes = 4;
        private TimeSpan _retryDelay = TimeSpan.FromSeconds(3);

        private bool _privateAPI = false;

        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();

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
                    _privateAPI = true;
                }
            }
            else
            {
                kc = new KrakenClient();    
            }

            kc.AddRateLimiter ( new RateLimiterAPIKey(15, TimeSpan.FromSeconds(3) ));
            
        }

        public virtual int RateLimiterPenality { get; private set; }


        public virtual void PreventRateLimit()
        {
            Task.Delay(RateLimiterPenality).Wait();
            RateLimiterPenality = 0;
        }


        public virtual Dictionary<string, decimal> Balances()
        {
            var bal = RetryHelper<Dictionary<string, decimal>>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetBalances() );
            RateLimiterPenality += 6000;
            return bal.Data;
        }

        public virtual double Balance(string asset = "ZEUR")
        {
            var bal = RetryHelper<KrakenTradeBalance>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetTradeBalance(asset) );
            RateLimiterPenality += 6000;
            return (double)bal.Data.CombinedBalance;
        }

        public virtual double MarketPrice(AssetName assetName)
        {
            var mk = RetryHelper<Dictionary<string, KrakenRestTick>>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetTickers(symbols: assetName.SymbolName) );
            RateLimiterPenality += 6000;
            return (double)mk.Data[assetName.SymbolName].LastTrade.Price;
        }

        public virtual KrakenTradesResult TradesHistory(AssetName assetName, DateTime dt)
        {
            var l = RetryHelper<KrakenTradesResult>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetRecentTrades(assetName.SymbolName, dt) );
            RateLimiterPenality += 6000;
            return l.Data;
        }

        /// <summary>
        /// Normalize a given symbol name, like "XRPEUR" to the equivalent for Kraken Symbol.
        /// </summary>
        /// <param name="symbol">The symbol name (ex: "XRPEUR")</param>
        /// <returns>A generic Asset name object. </returns>
        public virtual AssetName NormalizeSymbolName(string symbol)
        {
            var sym = RetryHelper<Dictionary<string, KrakenSymbol>>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetSymbols(symbols: symbol) );
            RateLimiterPenality += 3000;
            return new AssetName(sym.Data.First().Key, sym.Data.First().Value.BaseAsset, sym.Data.First().Value.QuoteAsset);
            //return new AssetName(sym.Data.First().Value.AlternateName, sym.Data.First().Value.BaseAsset, sym.Data.First().Value.QuoteAsset);
        }

        /// <summary>
        /// Return the prices history for q given asset
        /// </summary>
        /// <param name="assetName">An normalized asset built from a normalized symbol name.</param>
        /// <returns></returns>
        public virtual IEnumerable<double> PricesHistory(AssetName assetName)
        {
            // To Do: Return here the Prices History on the specified symbol
            List<double> db = new List<double>();
            return db;
        }

        public virtual Trade LatestTrade(AssetName assetName)
        {
            var mk = RetryHelper<KrakenUserTradesPage>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetTradeHistory() );
            RateLimiterPenality += 6000;
            var rt = mk.Data.Trades.First( x => x.Value.Symbol==assetName.SymbolName );
            
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
        public virtual bool Buy(AssetName assetName, double price, double ratio, bool dry)
        {
            if ( (ratio == 0) || (_privateAPI == false) )
            {
                return false;
            }

            var fiatSymbol = assetName.QuoteName;                   // "XRPEUR" => "ZEUR"
            var assetSymbol = assetName.SymbolName;
            var totalBalance = Balance(fiatSymbol);                 // Total balance of the portfolio on this exchange market
            var amount = (totalBalance*ratio)/100.0;                // Percentage of this total balance that can be used to place order.
            var bal = Balances();
            var availBalance = (double)bal[fiatSymbol];             // Available money for trading.
            if ( amount > availBalance)
            {
                amount = availBalance;
            }
            var qty = amount / price;

            if ( qty >= 30 )
            {
                _logger.Info("Place Order: Buy (" + assetSymbol + ") - Quantity: " + qty + " - Price:" + price + " - Total: " + amount + " " + assetName.QuoteName );
                var order = RetryHelper<KrakenPlacedOrder>.RetryOnException(_retryTimes, _retryDelay, () => kc.PlaceOrder(assetSymbol, OrderSide.Buy, OrderType.Market, quantity: (decimal)qty, validateOnly: dry) );
                RateLimiterPenality += 3000;
                return order.Success;
            }
            
            return false;
        }

        /// <summary>
        /// Place a "Sell" order on the Kraken exchange market.
        /// </summary>
        /// <param name="symbol">The crypto asset to sell for a given currency. ex: "BTCEUR" ></param>
        /// <param name="price">The wanted price (on the currency).</param>
        /// <param name="ratio">the pourcentage to qpply on the transaction.</param>
        /// <param name="dry">Is it for real or not?</param>
        /// <returns>true if the order was properly placed, false otherwise.</returns>
        public virtual bool Sell(AssetName assetName, double price, double ratio, bool dry)
        {
            if ( _privateAPI == false )
            {
                return false;
            }

            var bal = Balances();
            var qty = bal[assetName.BaseName];
            if ( qty > 0)
            {
                _logger.Info("Place Order: Sell (" + assetName.SymbolName + ") - Quantity: " + qty + " - Price:" + price + " - Total: " + (double)qty*price + " " + assetName.QuoteName );
                var order = RetryHelper<KrakenPlacedOrder>.RetryOnException(_retryTimes, _retryDelay, () => kc.PlaceOrder(assetName.SymbolName, OrderSide.Sell, OrderType.Market, quantity: (decimal)qty, validateOnly: dry) );
                RateLimiterPenality += 3000;
                return order.Success;
            }
            
            return false;
        }
        
        /// <summary>
        /// The name of the exchqnge market
        /// </summary>
        /// <returns></returns>
        public virtual string Name()
        {
            return "Kraken";
        }

        /// <summary>
        /// Return the fees for q Buy or Sell transaction.
        /// </summary>
        /// <param name="whole">The value to apply the fee on</param>
        /// <param name="oType">Buy or Sell order.</param>
        /// <returns></returns>
        public virtual double Fees(double whole, Trade.TOrderType oType)
        {
            var fee = oType == Trade.TOrderType.Buy?_krakenFeeBuy:_krakenFeeSell;
            return (whole * fee) / 100;
        }

    }
}
