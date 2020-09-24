using System;

namespace Cryptothune.Lib
{
    public class StrategyObject
    {
        public StrategyObject(IStrategy strategy, AssetSymbol assetName, double percent)
        {
            Strategy = strategy;
            AssetName = assetName;
            Percentage = percent;
        }

        public IStrategy Strategy { get; private set; }
        public AssetSymbol AssetName { get; private set; }
        public double Percentage { get; private set; }

    }
}

