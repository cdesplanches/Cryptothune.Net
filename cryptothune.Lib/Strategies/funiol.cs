using System;

namespace Cryptothune.Lib
{
    /// <summary>
    /// Funiol, the strangest trader ever.
    /// </summary>
    public class Funiol : IStrategy
    {
        /// <summary>
        /// ctor
        /// </summary>
        public Funiol()
        {
        }
        /// <summary>
        /// The name of this algo
        /// </summary>
        /// <returns></returns>
        public string Name()
        {
            return "Funiol, the strangest trading strategy ever.";
        }
        /// <summary>
        /// Help function that compute percentage
        /// </summary>
        /// <param name="part"></param>
        /// <param name="whole"></param>
        /// <returns></returns>
        private double Percentage( double part, double whole )
        {
            return part / (whole/100);
        }
        /// <summary>
        /// The 'brain' that decide if yes or no it must do an action.
        /// </summary>
        /// <param name="curPrice"></param>
        /// <param name="refPrice"></param>
        /// <param name="prevAction"></param>
        /// <returns></returns>
        public bool Decide(double curPrice, double refPrice, Trade.TOrderType prevAction)
        {
            double penality = 0.0f;
            double marketStatus = curPrice - refPrice;
            var p = Percentage(Math.Abs(marketStatus), curPrice);

            if (p>6.5)
                penality = 1.0; // 1.0
            else
            {
                if ((marketStatus>0 && (prevAction == Trade.TOrderType.Buy)) || (marketStatus <0 && (prevAction == Trade.TOrderType.Sell)))
                {
                    if (p>=2.5)
                    {
                        penality = 0.9; // 1.0
                    }
                }
            }
                
            Random rand = new Random();
            return (rand.NextDouble() < penality);
        }
    }
}
