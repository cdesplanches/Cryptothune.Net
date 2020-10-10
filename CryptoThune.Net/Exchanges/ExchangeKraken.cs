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



namespace CryptoThune.Net
{
    /// <summary>
    /// Kraken market object
    /// </summary>
    public class ExchangeKraken : IExchange
    {
        /// <summary>
        /// The internal Kraken client (from Kraken.net)
        /// </summary>
        private KrakenClient kc = null;
        private double _krakenFeeSell = 0.26;
        private double _krakenFeeBuy = 0.18;

        private int _retryTimes = 4;
        private TimeSpan _retryDelay = TimeSpan.FromSeconds(3);

        private bool _privateAPI = false;

        private static readonly Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        /// <summary>
        /// ctor
        /// </summary>
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
        /// <summary>
        /// The rate limiter penality (in ms)
        /// </summary>
        /// <value>The value in ms of the penality of rate limiter </value>
        public virtual int RateLimiterPenality { get; protected set; }
        /// <summary>
        /// Stop the execution on the process in order to respect the rate limiter.
        /// </summary>
        public virtual int PreventRateLimit()
        {
            var ms = RateLimiterPenality;
            Task.Delay(RateLimiterPenality).Wait();
            RateLimiterPenality = 0;

            return ms;
        }
        /// <summary>
        /// Reset the rate limit counter
        /// </summary>
        public virtual void ResetRateLimitCounter()
        {
            RateLimiterPenality = 0;
        }
        /// <summary>
        /// Get the current balance of all assets
        /// </summary>
        /// <returns></returns>
        public virtual Dictionary<string, decimal> Balances(DateTime? dt = null)
        {
            var bal = RetryHelper<Dictionary<string, decimal>>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetBalances() );
            RateLimiterPenality += 6000;
            return bal.Data;
        }
        /// <summary>
        /// Get the fiat balance (for a given currency)
        /// </summary>
        /// <param name="asset">the fiat currency (ex: ZEUR, or ZUSD)</param>
        /// <returns></returns>
        public virtual double Balance(string asset = "ZEUR")
        {
            var bal = RetryHelper<KrakenTradeBalance>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetTradeBalance(asset) );
            RateLimiterPenality += 6000;
            return (double)bal.Data.CombinedBalance;
        }
        /// <summary>
        /// The current price for a given asset
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        public virtual double MarketPrice(AssetSymbol assetName)
        {
            var mk = RetryHelper<Dictionary<string, KrakenRestTick>>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetTickers(symbols: assetName.SymbolName) );
            RateLimiterPenality += 6000;
            return (double)mk.Data[assetName.SymbolName].LastTrade.Price;
        }
        /// <summary>
        /// The recent trades history for a given symbol
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="dt"></param>
        /// <returns></returns>
        public virtual KrakenTradesResult TradesHistory(AssetSymbol assetName, DateTime dt)
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
        public virtual AssetSymbol NormalizeSymbolName(string symbol)
        {
            var sym = RetryHelper<Dictionary<string, KrakenSymbol>>.RetryOnException(_retryTimes, _retryDelay, () => kc.GetSymbols(symbols: symbol) );
            RateLimiterPenality += 3000;
            var asset = new AssetSymbol(sym.Data.First().Key, sym.Data.First().Value.BaseAsset, sym.Data.First().Value.QuoteAsset);
            asset.OrderMin = (double)sym.Data.First().Value.OrderMin;
            return asset;
        }
        /// <summary>
        /// Return the prices history for a given asset
        /// </summary>
        /// <param name="assetName">An normalized asset built from a normalized symbol name.</param>
        /// <returns></returns>
        public virtual Dictionary<double, double> PricesHistory(AssetSymbol assetName)
        {
            // To Do: Return here the Prices History on the specified symbol
            var db = new Dictionary<double, double>();
            return db;
        }
        /// <summary>
        /// Get the latest trades done for a given asset name
        /// </summary>
        /// <param name="assetName">The asset name to retreive the trade history on.</param>
        /// <returns></returns>
        public virtual Trade LatestTrade(AssetSymbol assetName)
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
        /// <param name="assetName"></param>
        /// <param name="price"></param>
        /// <param name="ratio"></param>
        /// <param name="dt"></param>
        /// <param name="dry"></param>
        /// <returns>true if the order is properly placed, false otherwise.</returns>
        public virtual bool Buy(AssetSymbol assetName, double price, double ratio, DateTime? dt, bool dry)
        {
            if ( (ratio == 0) || (_privateAPI == false) )
            {
                return false;
            }
            var dd = dt ?? DateTime.Now; 
            
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

            if ( qty >= assetName.OrderMin )
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
        /// <param name="assetName">The crypto asset to sell for a given currency. ex: "BTCEUR" ></param>
        /// <param name="price">The wanted price (on the currency).</param>
        /// <param name="ratio">the pourcentage to qpply on the transaction.</param>
        /// <param name="dt">When schedule that order.</param>
        /// <param name="dry">Is it for real or not?</param>
        /// <returns>true if the order was properly placed, false otherwise.</returns>
        public virtual bool Sell(AssetSymbol assetName, double price, double ratio, DateTime? dt, bool dry)
        {
            if ( _privateAPI == false )
            {
                return false;
            }
            var dd = dt ?? DateTime.Now; 

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
