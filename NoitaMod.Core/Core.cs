using System;
using System.Diagnostics;
using NoitaMod.Log;
using NoitaMod.Memory;

namespace NoitaMod.Core
{
    public static class Core
    {
        [DllExport]
        public static void Entry()
        {
            try
            {
                Init();
            }
            catch ( Exception ex )
            {
                if ( !EventLog.SourceExists( "NoitaMod" ) )
                {
                    EventLog.CreateEventSource( "NoitaMod", "NoitaModLog" );
                }

                var eventLog = new EventLog();
                eventLog.Source = "NoitaMod";
                eventLog.Log = "NoitaModLog";
                eventLog.WriteEntry( ex.Message, EventLogEntryType.Error );
                eventLog.WriteEntry( ex.StackTrace, EventLogEntryType.Error );

                throw ( ex ); // crash
            }
        }

        private static void Init()
        {
            Logger.Instance.DeleteLog();
            Logger.Instance.WriteLine( "NoitaMod.Core.Init()" );

            Process process = Process.GetCurrentProcess();
            process.EnableRaisingEvents = true;
            process.Exited += ( sender, e ) =>
            {
                Logger.Instance.WriteLine( $"Process exit: {process.ExitCode.ToString()}" );
                Logger.Instance.Dispose();
            };

            Handles.Init( process.Handle );
            Scanner.Instance.Init( "noita.exe" );

            // Testing
            var pattern = "6c 69 67 68 74 6e 69 6e 67 5f 63 6f 75 6e 74 00 6d 61 74 65 72 69 61 6c";
            var addr = Scanner.Instance.FindPattern(pattern);
            Logger.Instance.WriteLine( $"Pattern: {pattern}" );
            Logger.Instance.WriteLine( $"Pattern addr: 00x{addr.ToString( "X8" )}" );
        }
    }
}
