using System;

namespace Cryptothune.Lib
{
    public class Funiol : IStrategy
    {
        public Funiol()
        {
        }
        
        public string Name()
        {
            return "Funiol, the strangest trading strategy ever.";
        }

        private double Percentage( double part, double whole )
        {
            return part / (whole/100);
        }

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
