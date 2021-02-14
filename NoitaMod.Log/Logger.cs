using System;
using System.IO;
using NoitaMod.Util;
using NoitaMod.Common;

namespace NoitaMod.Log
{   
    public class Logger : Singleton<Logger>, ILogger, IDisposable
    {
        static string defaultLogPath = "noitamod.log";
        string logPath = defaultLogPath;
        static string dateFormat = "yyyy-MM-dd HH:mm:ss.ff";
        FileStream logStream;
        StreamWriter logWriter;

        StreamWriter LogWriter
        {
            get
            {
                if ( logStream == null )
                {
                    logStream = new FileStream( logPath, FileMode.OpenOrCreate, FileAccess.Write );
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
            Close();

            instance = null;
        }

        private void Close()
        {
            if ( logWriter != null )
            {
                logWriter.Flush();
                logWriter.Dispose();
                logWriter.Close();
                logWriter = null;
            }

            if ( logStream != null )
            {
                if ( logStream.CanWrite )
                {
                    logStream.Flush();
                }
                logStream.Dispose();
                logStream.Close();
                logStream = null;
            }
        }

        public void DeleteLog()
        {
            Close();
            if ( File.Exists( logPath ) )
            {
                File.Delete( logPath );
            }
        }

        public void SetLogPath( string logPath )
        {
            this.logPath = logPath;
        }
    }
}
