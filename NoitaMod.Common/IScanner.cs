using System.Collections.Generic;

namespace NoitaMod.Common
{
    public interface IScanner
    {
        void AddPattern( string patternName, string pattern );
        ulong FindPattern( string pattern );
        Dictionary<string, ulong> FindPatterns();
    }
}
