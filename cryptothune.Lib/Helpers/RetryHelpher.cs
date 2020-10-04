using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CryptoExchange.Net.Objects;

namespace Cryptothune.Lib
{
    /// <summary>
    /// Helper class about retries on case of errors
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public static class RetryHelper<T>
    {
        /// <summary>
        /// Retry on exception
        /// </summary>
        /// <param name="times">Nb of retry</param>
        /// <param name="delay">Delay betzeen 2 retry</param>
        /// <param name="operation">A void func to call if success.</param>
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
        /// <summary>
        /// Retry on error
        /// </summary>
        /// <param name="times">Nb retry</param>
        /// <param name="delay">delay to wait between 2 retries</param>
        /// <param name="operation">the function to call.</param>
        /// <returns></returns>
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