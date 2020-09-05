using System;

namespace Cryptothune.Lib
{
    public interface IStrategy
    {
        string Name();
        
        bool Decide(double curPrice, double refPrice, Trade.TOrderType prevAction);
    }
}
