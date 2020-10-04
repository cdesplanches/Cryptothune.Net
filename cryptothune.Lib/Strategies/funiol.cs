using System;

namespace Cryptothune.Lib
{
    /// <summary>
    /// Funiol, the strangest trader ever.
    /// </summary>
    public class Funiol : IStrategy
    {
        private double _threshold;
        private double _ruptor;
        private double _proba;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="threshold">Threshold to trig the decision</param>
        /// <param name="ruptor">% to force a decision</param>
        /// <param name="proba">The probability to take a decision</param>
        public Funiol(double threshold=2.5, double ruptor=6.5, double proba=0.9)
        {
            _threshold = threshold;
            _ruptor = ruptor;
            _proba = proba;
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

            if (p>_ruptor)
                penality = 1.0;
            else
            {
                if ((marketStatus>0 && (prevAction == Trade.TOrderType.Buy)) || (marketStatus <0 && (prevAction == Trade.TOrderType.Sell)))
                {
                    if (p>=_threshold)
                    {
                        penality = _proba;
                    }
                }
            }
                
            Random rand = new Random();
            return (rand.NextDouble() < penality);
        }
    }
}
