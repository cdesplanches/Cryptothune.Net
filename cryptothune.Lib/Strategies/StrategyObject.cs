using System;

namespace Cryptothune.Lib
{
    /// <summary>
    /// Object that contains a strategy linked to a given asset.
    /// </summary>
    public class StrategyObject
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="assetName"></param>
        /// <param name="percent"></param>
        public StrategyObject(IStrategy strategy, AssetSymbol assetName, double percent)
        {
            Strategy = strategy;
            AssetName = assetName;
            Percentage = percent;
        }
        /// <summary>
        /// the strategy linkedto that object
        /// </summary>
        /// <value></value>
        public IStrategy Strategy { get; private set; }
        /// <summary>
        /// The asset
        /// </summary>
        /// <value></value>
        public AssetSymbol AssetName { get; private set; }
        /// <summary>
        /// The percentage of total money to apply on the asset for the given strategy.
        /// </summary>
        /// <value></value>
        public double Percentage { get; private set; }

    }
}

