using System;
using System.Collections.Generic;
using NUnit.Framework;
using CryptoThune.Net.Objects;
using CryptoThune.Net.Converters;
using Newtonsoft.Json;

namespace CryptoThune.Net.Tests
{
    [TestFixture]
    public class PortfolioTests
    {
        [TestCase()]
        public void TestPortfolio()
        {
            PortfolioEntry pt = new PortfolioEntry();
            string json = JsonConvert.SerializeObject(pt, Formatting.Indented, new PortfolioConverter());

            Assert.IsNotEmpty (json);
        }
    }
}   