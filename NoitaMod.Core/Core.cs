using System;
using System.Diagnostics;
using NoitaMod.Log;
using NoitaMod.Memory;
using NoitaMod.Plugin;
using NoitaMod.Common;

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
                // NoitaMod.Log may not be loaded
                try
                {
                    Logger.Instance.WriteLine( ex.Message, LogLevel.Error );
                    Logger.Instance.WriteLine( ex.StackTrace, LogLevel.Error );
                }
                catch ( Exception )
                {
                    // ignore
                }

                // Log to EventLog
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
            Logger.Instance.WriteLine( "NoitaMod.Core.Core.Init()" );

            Process process = Process.GetCurrentProcess();

            Handles.Init( process.Handle );
            Scanner.Instance.Init( "noita.exe" );
            Loader.Instance.Init();
        }
    }
}
