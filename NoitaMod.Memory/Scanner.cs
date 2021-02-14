using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NoitaMod.Log;
using NoitaMod.Util;

namespace NoitaMod.Memory
{
    // Signature scanning based on SigScanSharp by Striekcarl/GENESIS @ Unknowncheats
    // https://gist.github.com/vmcall/18cf27339c44e785bae247eae88e2844

    // Usage:
    //
    // Find Patterns (Simultaneously):
    //   Sigscan.AddPattern("Pattern1", "48 8D 0D ? ? ? ? E8 ? ? ? ? E8 ? ? ? ? 48 8B D6");
    //   Sigscan.AddPattern("Pattern2", "E8 0A EC ? ? FF");
    //    
    //   var result = Sigscan.FindPatterns();
    //   var offset = result["Pattern1"];
    //    
    // Find Patterns (Individual):
    //   var offset = Sigscan.FindPattern("48 8D 0D ? ? ? ? E8 ? ? ? ? E8 ? ? ? ? 48 8B D6");

    public class Scanner : Singleton<Scanner>
    {
        private byte[] moduleBuffer;
        private ulong moduleBase;
        private Dictionary<string, string> stringPatterns;
        private ProcessModule targetModule;

        public void Init( string moduleName )
        {
            Logger.Instance.WriteLine( "NoitaMod.Memory.Scanner.Init()" );
            stringPatterns = new Dictionary<string, string>();

            ProcessModuleCollection modules = Operations.GetProcessModules();
            foreach ( ProcessModule m in modules )
            {
                if ( m.ModuleName == moduleName )
                {
                    targetModule = m;
                }
            }

            moduleBuffer = new byte[targetModule.ModuleMemorySize];
            moduleBase = ( ulong )targetModule.BaseAddress;
            Operations.Read( moduleBase, moduleBuffer, moduleBuffer.Length );

            Logger.Instance.WriteLine( $"  loaded module: {targetModule.ModuleName}, size: {targetModule.ModuleMemorySize}" );
        }

        public void AddPattern( string patternName, string pattern )
        {
            stringPatterns.Add( patternName, pattern );
        }

        private bool PatternCheck( int offset, byte[] arrPattern )
        {
            for ( int i = 0; i < arrPattern.Length; i++ )
            {
                if ( arrPattern[i] == 0x0 )
                {
                    continue;
                }

                if ( arrPattern[i] != moduleBuffer[offset + i] )
                {
                    return false;
                }
            }

            return true;
        }

        public ulong FindPattern( string pattern )
        {
            if ( moduleBuffer == null || moduleBase == 0 )
            {
                throw new Exception( "No module" );
            }

            var patternBytes = ParsePatternString(pattern);

            for ( int i = 0; i < moduleBuffer.Length; i++ )
            {
                if ( moduleBuffer[i] != patternBytes[0] )
                {
                    continue;
                }

                if ( PatternCheck( i, patternBytes ) )
                {
                    return moduleBase + ( ulong )i;
                }
            }

            return 0;
        }

        public Dictionary<string, ulong> FindPatterns()
        {
            if ( moduleBuffer == null || moduleBase == 0 )
            {
                throw new Exception( "No module" );
            }

            byte[][] bytePatterns = new byte[stringPatterns.Count][];
            ulong[] results = new ulong[stringPatterns.Count];

            // parse patterns
            for ( int i = 0; i < stringPatterns.Count; i++ )
            {
                bytePatterns[i] = ParsePatternString( stringPatterns.ElementAt( i ).Value );
            }

            // scan
            for ( int i = 0; i < moduleBuffer.Length; i++ )
            {
                for ( int j = 0; j < bytePatterns.Length; j++ )
                {
                    if ( results[j] != 0 )
                    {
                        continue;
                    }

                    if ( PatternCheck( i, bytePatterns[j] ) )
                    {
                        results[j] = moduleBase + ( ulong )i;
                    }
                }
            }

            // format
            Dictionary<string, ulong> formatted = new Dictionary<string, ulong>();

            for ( int i = 0; i < bytePatterns.Length; i++ )
            {
                formatted[stringPatterns.ElementAt( i ).Key] = results[i];
            }

            return formatted;
        }

        private byte[] ParsePatternString( string pattern )
        {
            List<byte> patternBytes = new List<byte>();

            foreach ( var b in pattern.Split( ' ' ) )
            {
                patternBytes.Add( ( b == "?" || b == "??" ) ? ( byte )0x0 : Convert.ToByte( b, 16 ) );
            }

            return patternBytes.ToArray();
        }
    }
}
