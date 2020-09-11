using System;


namespace Cryptothune.Lib
{
    public class Trade
    {
        public enum TOrderType { Buy = 1, Sell = 2 };

        public Trade( )
        {
        }

        public double RefPrice { get; set; }

        public TOrderType OrderType { get; set; }
    }
}