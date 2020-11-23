using System;
using CryptoExchange.Net.Converters;
using CryptoThune.Net.Converters;
using Newtonsoft.Json;

namespace CryptoThune.Net.Objects
{
    /// <summary>
    /// Portfolio entry
    /// </summary>
    public class PortfolioEntry
    {
        /// <summary>
        /// The asset of the portfolio
        /// </summary>
        /// <value></value>
        [JsonConverter(typeof(AssetSymbolConverter))]
        public AssetSymbol Asset { get; set; }
        /// <summary>
        /// The weight the asset has on the portfolio
        /// </summary>
        /// <value></value>
        public decimal Weight { get; set; }
        /// <summary>
        /// The strategy applied to the asset
        /// </summary>
        /// <value></value>
        [JsonConverter(typeof(StrategyObjectConverter))]
        public StrategyObject StrategyDef { get; set; }
    }
}
