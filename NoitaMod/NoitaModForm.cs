using System;
using System.Linq;
using System.Windows.Forms;
using System.Diagnostics;
using System.Timers;
using NoitaMod.Log;

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
            Logger.Instance.WriteLine( "NoitaModForm.injectDLL()" );
            DLLInjectionResult result = Injector.Instance.Inject(processName, @"NoitaMod.Core.dll");
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
            Logger.Instance.WriteLine( $"NoitaModForm.injectDLL result {result}" );
        }

        private void doInjections()
        {
            bool isRunning = Process.GetProcessesByName(processName).Length > 0;
            if ( injectNextTick )
            {
                // Do injection
                Process process = Process.GetProcessesByName(processName).First();
                if ( !process.Responding )
                {
                    processCheckTimer.Interval = 3000;
                    return;
                }
                injectDLL();
                injectNextTick = false;
                processCheckTimer.Interval = 2000;
            }
            else if ( isRunning )
            {
                if ( !isInjected )
                {
                    if ( startedAfterMod )
                    {
                        // Give a few seconds for game to start
                        processCheckTimer.Interval = 3000;
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
            Logger.Instance.WriteLine( "NoitaModForm.NoitaModForm()" );
            InitializeComponent();
        }

        private void NoitaModForm_Load( object sender, EventArgs e )
        {
            Logger.Instance.WriteLine( "NoitaModForm.NoitaModForm_Load()" );
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
