using System;
using System.Collections.Generic;

namespace CryptoThune.Net
{
    /// <summary>
    /// Interface for an exchange market
    /// </summary>
    public interface IExchange
    {
        /// <summary>
        /// Normalize a symbol asset
        /// </summary>
        /// <param name="symbol"></param>
        /// <returns></returns>
        AssetSymbol NormalizeSymbolName(string symbol);
        /// <summary>
        /// Fees for a transaction
        /// </summary>
        /// <param name="whole"></param>
        /// <param name="oType"></param>
        /// <returns></returns>
        double Fees(double whole, Trade.TOrderType oType);
        /// <summary>
        /// The complete balances
        /// </summary>
        /// <returns></returns>
        Dictionary<string, decimal> Balances(DateTime? dt = null);
        /// <summary>
        /// The balance for a given asset
        /// </summary>
        /// <param name="asset"></param>
        /// <returns></returns>
        double Balance(string asset);
        /// <summary>
        /// The price history
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>        
        Dictionary<double,double> PricesHistory(AssetSymbol assetName);
        /// <summary>
        /// the current market price of a given asset
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        double MarketPrice(AssetSymbol assetName);
        /// <summary>
        /// The last tarde transaction
        /// </summary>
        /// <param name="assetName"></param>
        /// <returns></returns>
        Trade LatestTrade(AssetSymbol assetName);
        /// <summary>
        /// Place a buy order
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="price"></param>
        /// <param name="qty"></param>
        /// <param name="dt"></param>
        /// <param name="dry"></param>
        /// <returns></returns>
        bool Buy (AssetSymbol assetName, double price, double qty = 100.0, DateTime? dt = null, bool dry = true);
        /// <summary>
        /// Place a sell order
        /// </summary>
        /// <param name="assetName"></param>
        /// <param name="price"></param>
        /// <param name="qty"></param>
        /// <param name="dt"></param>
        /// <param name="dry"></param>
        /// <returns></returns>
        bool Sell (AssetSymbol assetName, double price, double qty = 100.0, DateTime? dt = null, bool dry = true);
        /// <summary>
        /// Pause the execution according to the current rate limiter counter.
        /// </summary>
        int PreventRateLimit();
        /// <summary>
        /// Reset the rate limit counter
        /// </summary>
        void ResetRateLimitCounter();
    }
}
