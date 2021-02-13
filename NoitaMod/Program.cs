using System;
using System.Windows.Forms;

namespace NoitaMod
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.SetUnhandledExceptionMode( UnhandledExceptionMode.CatchException );
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault( false );
            Application.Run( new NoitaModForm() );
        }
    }
}
