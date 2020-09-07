using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cryptothune.Lib
{
    public static class RetryHelper<T>
    {
        public static void RetryOnException(int times, TimeSpan delay, Action operation)
        {
            var remainingTries = times;  
            var exceptions = new List<Exception>();

            do
            {
                --remainingTries;
                try
                {
                    operation();
                    break; // Sucess! Lets exit the loop!
                }
                catch (Exception e)
                {               
                    exceptions.Add(e);                            
                    Task.Delay(delay).Wait();
                }
            } while (remainingTries > 0);

            throw new AggregateException(exceptions);
        }


        public static T RetryOnException(int times, TimeSpan delay, Func<T> operation)
        {
            var remainingTries = times;
            var wait = delay;
            var exceptions = new List<Exception>();

            do
            {
                --remainingTries;
                try
                {
                    return operation();
                }
                catch (Exception e)
                {              
                    Console.WriteLine("Error: Retrying...");
                    exceptions.Add(e);
                    Task.Delay(wait).Wait();
                    wait += delay;
                }
            } while (remainingTries > 0);

            throw new AggregateException(exceptions);
        }
    }
}