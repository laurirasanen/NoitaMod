using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Timers;
using NoitaMod.Log;
using System.IO;

namespace NoitaMod
{
    struct StatusStrings
    {
        public static readonly string UNINJECTED = "Uninjected";
        public static readonly string PROCESS_NOT_ACTIVE = "Uninjected (Noita is not running)";
        public static readonly string INSTALLATION_WRONG = "Uninjected (Could not find NoitaMod.Core.dll)";
        public static readonly string WAITING_FOR_LOAD = "Waiting for Noita to load";
        public static readonly string INJECTED = "Injected";
        public static readonly string INJECTION_FAILED = "Injection failed";
    }

    public partial class NoitaModForm : Form
    {
        Boolean isInjected = false;
        Boolean startedAfterMod = false;
        bool injectNextTick = false;
        bool isInjecting = false;

        private String injectionStatus = "";
        private System.Timers.Timer processCheckTimer;
        private static string processName = "noita";

        String InjectionStatus
        {
            get
            {
                return injectionStatus;
            }
            set
            {
                injectionStatus = value;
                statusLabel.Invoke( ( Action )( () => statusLabel.Text = injectionStatus ) );
            }
        }

        void injectDLL()
        {
            if ( isInjecting )
            {
                return;
            }

            isInjecting = true;

            Logger.Instance.WriteLine( "NoitaMod.NoitaModForm.injectDLL()" );
            var Dlls = new string[][]{
                //new string[]{ $@"{Directory.GetCurrentDirectory()}\NoitaMod.Log.dll"},
                //new string[]{ $@"{Directory.GetCurrentDirectory()}\NoitaMod.API.dll"},
                //new string[]{ $@"{Directory.GetCurrentDirectory()}\NoitaMod.Memory.dll"},
                new string[]{ $@"{Directory.GetCurrentDirectory()}\NoitaMod.Core.dll", "Entry"},
            };

            for ( int i = 0; i < Dlls.Length; i++ )
            {
                DLLInjectionResult result = Injector.Instance.Inject(processName, Dlls[i][0], Dlls[i].Length > 1 ? Dlls[i][1] : "");
                if ( i == Dlls.Length - 1 )
                {
                    switch ( result )
                    {
                        case DLLInjectionResult.DLL_NOT_FOUND:
                            InjectionStatus = StatusStrings.INSTALLATION_WRONG;
                            break;
                        case DLLInjectionResult.GAME_PROCESS_NOT_FOUND:
                            InjectionStatus = StatusStrings.PROCESS_NOT_ACTIVE;
                            break;
                        case DLLInjectionResult.INJECTION_FAILED:
                            InjectionStatus = StatusStrings.INJECTION_FAILED;
                            break;
                        case DLLInjectionResult.SUCCESS:
                            InjectionStatus = StatusStrings.INJECTED;
                            isInjected = true;
                            break;
                    }
                }
                Logger.Instance.WriteLine( $"NoitaMod.NoitaModForm.injectDLL {Dlls[i][0]} {result}" );
            }

            isInjecting = false;
        }

        private void doInjections()
        {
            if ( isInjecting )
            {
                return;
            }

            bool isRunning = Process.GetProcessesByName(processName).Length > 0;
            if ( injectNextTick )
            {
                // Do injection
                Process process = Process.GetProcessesByName(processName).First();
                if ( !process.Responding )
                {
                    return;
                }
                injectDLL();
                injectNextTick = false;
            }
            else if ( isRunning )
            {
                if ( !isInjected )
                {
                    if ( startedAfterMod )
                    {
                        // Give a few seconds for game to start
                        injectNextTick = true;
                        InjectionStatus = StatusStrings.WAITING_FOR_LOAD;

                    }
                    else
                    {
                        injectDLL();
                    }
                }
            }
            else
            {
                isInjected = false;
                startedAfterMod = true;
                InjectionStatus = StatusStrings.PROCESS_NOT_ACTIVE;
            }
        }

        public NoitaModForm()
        {
            Logger.Instance.SetLogPath( "noitamod-injector.log" );
            Logger.Instance.DeleteLog();
            Logger.Instance.WriteLine( "NoitaMod.NoitaModForm.NoitaModForm()" );
            InitializeComponent();
        }

        private void NoitaModForm_Load( object sender, EventArgs e )
        {
            Logger.Instance.WriteLine( "NoitaMod.NoitaModForm.NoitaModForm_Load()" );
            InjectionStatus = StatusStrings.UNINJECTED;
            processCheckTimer = new System.Timers.Timer( 2000 );
            processCheckTimer.Elapsed += new ElapsedEventHandler( timer_Tick );
            processCheckTimer.Start();
        }

        private void timer_Tick( object sender, EventArgs e )
        {
            doInjections();
        }
    }
}
