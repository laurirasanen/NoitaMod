using System;
using System.Diagnostics;
using NoitaMod.Log;

namespace NoitaMod.Core
{
    public static class Core
    {
        [DllExport]
        public static void Entry()
        {
            Logger.Instance.WriteLine( "NoitaMod.Core.Entry()" );

            Process process = Process.GetCurrentProcess();
            process.EnableRaisingEvents = true;
            process.Exited += ( sender, e ) =>
            {
                Logger.Instance.WriteLine( $"Process exit: {process.ExitCode.ToString()}" );
                Logger.Instance.Dispose();
            };

            Memory.Handles.Init( process.Handle );
        }
    }
}
