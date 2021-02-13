using System;
using System.IO;

namespace NoitaMod.Log
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }

    public class Logger : IDisposable
    {
        static Logger instance;
        public static Logger Instance
        {
            get
            {
                if ( instance == null )
                {
                    instance = new Logger();
                }

                return instance;
            }
        }

        static string logPath = "noitamod.log";
        static string dateFormat = "yyyy-MM-dd HH:mm:ss.ff";
        FileStream logStream;
        StreamWriter logWriter;

        StreamWriter LogWriter
        {
            get
            {
                if ( logStream == null )
                {
                    logStream = new FileStream( logPath, FileMode.Create );
                    logWriter = new StreamWriter( logStream );
                    logWriter.AutoFlush = true;
                }

                return logWriter;
            }
        }

        public void WriteLine( string text, LogLevel level = LogLevel.Debug )
        {
            string timeStamp = DateTime.Now.ToString(dateFormat);
            LogWriter.Write( $"[{timeStamp}] [{level.ToString( "g" ).ToUpper()}] {text}\n" );
        }

        public void Dispose()
        {
            if ( logWriter != null )
            {
                logWriter.Flush();
                logWriter.Dispose();
            }

            if ( logStream != null )
            {
                logStream.Flush();
                logStream.Dispose();
            }

            instance = null;
        }
    }
}
