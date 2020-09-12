using System;


namespace Cryptothune.Lib
{
    public class AssetName
    {
        public AssetName( string symbolName, string baseName, string quoteName )
        {
            SymbolName = symbolName;
            BaseName = baseName;
            QuoteName = quoteName;
        }

        public string QuoteName { get; private set; }
        public string BaseName { get; private set; }
        public string SymbolName { get; private set; }
    }
}