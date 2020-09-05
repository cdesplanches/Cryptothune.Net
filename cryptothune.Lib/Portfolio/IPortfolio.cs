using System;
using System.Data.SQLite;
using System.Collections.Generic;

namespace Cryptothune.Lib
{
    public interface IPortfolio
    {
        void Export();

        bool Buy(string symbol, double marketPrice, bool dry);

        bool Sell(string symbol, double marketPrice, bool dry);
    }
}