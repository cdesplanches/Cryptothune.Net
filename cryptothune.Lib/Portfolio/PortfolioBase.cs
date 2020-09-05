using System;

namespace Cryptothune.Lib
{
    public abstract class PortfolioBase : IPortfolio
    {
        public PortfolioBase(IExchange exchange)
        {
            this.MarketExchange = exchange;
        }
        
        public IExchange MarketExchange { get; protected set; }

        public virtual void Export()
        {

        }

        public virtual bool Buy(string symbol, double marketPrice = 0.0, bool dry = true )
        {
            return false;
        }

        public virtual bool Sell(string symbol, double marketPrice = 0.0, bool dry = true )
        {
            return false;
        }
    }
}