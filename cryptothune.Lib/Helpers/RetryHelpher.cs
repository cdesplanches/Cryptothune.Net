using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;

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


        public static WebCallResult<T> RetryOnException(int times, TimeSpan delay, Func<WebCallResult<T>> operation)
        {
            var remainingTries = times;
            var wait = delay;
            do
            {
                --remainingTries;
                var ret = operation();
                if (ret.Success)
                {
                    return ret;
                }
                else
                {
                    if (ret.Error.Code == 3 ) // Rate Limit Error
                    {              
                        Console.WriteLine("Error: Retrying...");
                        Task.Delay(wait).Wait();
                        wait += delay;
                    }
                }
            } while (remainingTries > 0);

            throw new Exception("API Call Error!!");
        }
    }
}