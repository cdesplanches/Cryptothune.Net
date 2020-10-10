using System;
using System.Collections.Generic;


namespace CryptoThune.Net
{
    /// <summary>
    /// Define what is a symbol
    /// </summary>
    public class AssetSymbol
    {
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="symbolName"></param>
        /// <param name="baseName"></param>
        /// <param name="quoteName"></param>
        public AssetSymbol( string symbolName, string baseName, string quoteName )
        {
            SymbolName = symbolName;
            BaseName = baseName;
            QuoteName = quoteName;

            OrderMin = 1.0;
        }
        /// <summary>
        /// The Quote name of a symbol (ex: ZEUR)
        /// </summary>
        /// <value></value>
        public string QuoteName { get; private set; }
        /// <summary>
        /// The base name of the symbol (ex: XBT)
        /// </summary>
        /// <value></value>
        public string BaseName { get; private set; }
        /// <summary>
        /// The complete symbol name. (ex: XXBTEUR)
        /// </summary>
        /// <value></value>
        public string SymbolName { get; private set; }
        /// <summary>
        /// The minimum order authorized for this asset
        /// </summary>
        /// <value></value>
        public double OrderMin { get; set; }
    }
}