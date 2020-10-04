using System;

namespace Cryptothune.Lib
{
    /// <summary>
    /// Strategy interface
    /// </summary>
    public interface IStrategy
    {
        /// <summary>
        /// The name of this strategy
        /// </summary>
        /// <returns></returns>
        string Name();
        /// <summary>
        /// The algo to implement
        /// </summary>
        /// <param name="curPrice"></param>
        /// <param name="refPrice"></param>
        /// <param name="prevAction"></param>
        /// <returns></returns>
        bool Decide(double curPrice, double refPrice, Trade.TOrderType prevAction);
    }
}
