using System;

namespace Cryptothune.Lib
{
    /// <summary>
    /// A trade object
    /// </summary>
    public class Trade
    {
        /// <summary>
        /// Buy, or Sell
        /// </summary>
        public enum TOrderType 
        { 
            /// <summary>
            /// A Buy trade
            /// </summary>
            Buy = 1, 
            /// <summary>
            /// A Sell trade
            /// </summary>
            Sell = 2 
        };
        /// <summary>
        /// ctor
        /// </summary>
        public Trade( )
        {
            RefPrice = 0.0;
            OrderType = TOrderType.Sell;
            Quantity = 0.0;
            Asset = null;
            Timestamp = DateTime.Now;
        }
        /// <summary>
        /// The date time when the trade occur
        /// </summary>
        /// <value></value>
        public DateTime Timestamp{ get; set; }
        /// <summary>
        /// the quantity of asset trade.
        /// </summary>
        /// <value></value>
        public double Quantity { get; set; }
        /// <summary>
        /// The trade asset
        /// </summary>
        /// <value></value>
        public AssetSymbol Asset { get; set; }
        /// <summary>
        /// The reference price
        /// </summary>
        /// <value></value>
        public double RefPrice { get; set; }
        /// <summary>
        /// Buy or Sell?
        /// </summary>
        /// <value></value>
        public TOrderType OrderType { get; set; }
    }
}