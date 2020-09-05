using System;


namespace Cryptothune.Lib
{
    public struct Trade
    {
        [Flags]
        public enum TOrderType { Buy = 1, Sell = 2 };

        public double RefPrice { get; set; }

        public TOrderType OrderType { get; set; }
    }
}