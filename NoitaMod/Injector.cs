using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using NoitaMod.Log;

namespace NoitaMod
{
    public enum DLLInjectionResult
    {
        DLL_NOT_FOUND,
        GAME_PROCESS_NOT_FOUND,
        INJECTION_FAILED,
        SUCCESS
    }

    public sealed class Injector
    {
        // Externs
        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern int WriteProcessMemory( IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten );

        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern IntPtr GetProcAddress( IntPtr hModule, string lpProcName );

        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern IntPtr CreateRemoteThread( IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress,
            IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId );

        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern uint WaitForSingleObject( IntPtr hHandle, uint dwMilliseconds );

        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern IntPtr VirtualAllocEx( IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect );

        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern IntPtr GetModuleHandle( string lpModuleName );

        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern bool GetExitCodeThread( IntPtr hThread, out uint lpExitCode );

        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern IntPtr OpenProcess( uint dwDesiredAccess, int bInheritHandle, uint dwProcessId );

        [DllImport( "kernel32", SetLastError = true, CharSet = CharSet.Ansi )]
        static extern IntPtr LoadLibrary( [MarshalAs( UnmanagedType.LPStr )] string lpFileName );

        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern int CloseHandle( IntPtr hObject );

        [DllImport( "kernel32.dll", SetLastError = true )]
        static extern bool SetCurrentDirectory( string lpPathName );

        [DllImport( "kernel32.dll" )]
        static extern uint GetCurrentDirectory( uint nBufferLength, [Out] StringBuilder lpBuffer );

        static readonly IntPtr INTPTR_ZERO = IntPtr.Zero;
        static readonly uint desiredAccess = (0x2 | 0x8 | 0x10 | 0x20 | 0x400);

        private Injector() { }

        public DLLInjectionResult Inject( string processName, string dllPath, string functionToCall = "" )
        {
            if ( !File.Exists( dllPath ) )
            {
                return DLLInjectionResult.DLL_NOT_FOUND;
            }

            uint processId = 0;

            Process[] processes = Process.GetProcesses();
            foreach ( Process p in processes )
            {
                if ( p.ProcessName == processName )
                {
                    processId = ( uint )p.Id;
                }
            }

            if ( processId == 0 )
            {
                return DLLInjectionResult.GAME_PROCESS_NOT_FOUND;
            }

            if ( !injectDLL( processId, dllPath, functionToCall ) )
            {
                return DLLInjectionResult.INJECTION_FAILED;
            }

            return DLLInjectionResult.SUCCESS;
        }

        bool injectDLL( uint processToInject, string dllPath, string functionToCall = "" )
        {
            IntPtr processHandle = OpenProcess(desiredAccess, 1, processToInject);
            if ( processHandle == INTPTR_ZERO )
            {
                return false;
            }

            IntPtr loadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");

            if ( loadLibraryAddress == INTPTR_ZERO )
            {
                return false;
            }

            IntPtr argAddress = VirtualAllocEx(processHandle, (IntPtr)null, (IntPtr)dllPath.Length, (0x1000 | 0x2000), 0X40);

            if ( argAddress == INTPTR_ZERO )
            {
                return false;
            }

            byte[] bytes = Encoding.ASCII.GetBytes(dllPath);

            if ( WriteProcessMemory( processHandle, argAddress, bytes, ( uint )bytes.Length, 0 ) == 0 )
            {
                return false;
            }

            IntPtr moduleHandle = CreateRemoteThread( processHandle, ( IntPtr )null, INTPTR_ZERO, loadLibraryAddress, argAddress, 0, ( IntPtr )null );

            if ( moduleHandle == INTPTR_ZERO )
            {
                return false;
            }

            WaitForSingleObject( moduleHandle, unchecked(( uint )-1) );

            uint moduleAddress;
            GetExitCodeThread( moduleHandle, out moduleAddress );

            if ( moduleAddress == 0 )
            {
                Logger.Instance.WriteLine( "Failed to get thread exit code / module address" );
                return false;
            }

            Logger.Instance.WriteLine( $"Module address: {moduleAddress}" );

            if ( functionToCall.Length == 0 )
            {
                CloseHandle( processHandle );
                return true;
            }

            var lib = LoadLibrary(dllPath);
            IntPtr entryAddress = GetProcAddress(lib, functionToCall);
            if ( entryAddress == INTPTR_ZERO )
            {
                Logger.Instance.WriteLine( "Failed to find Entry address" );
                return false;
            }
            Process proc = Process.GetCurrentProcess();
            IntPtr moduleBase = INTPTR_ZERO;
            foreach ( ProcessModule m in proc.Modules )
            {
                if ( dllPath.Contains( m.ModuleName ) )
                {
                    moduleBase = m.BaseAddress;
                    break;
                }
            }

            IntPtr entryOffset = ( IntPtr )( ( ulong )entryAddress - ( ulong )moduleBase );
            Logger.Instance.WriteLine( $"entryOffset {entryOffset}" );

            entryAddress = ( IntPtr )( ( ulong )moduleAddress + ( ulong )entryOffset );
            Logger.Instance.WriteLine( $"Entry {entryAddress}" );

            IntPtr entryResult = CreateRemoteThread( processHandle, ( IntPtr )null, INTPTR_ZERO, entryAddress, (IntPtr)null, 0, ( IntPtr )null );

            if ( entryResult == INTPTR_ZERO )
            {
                return false;
            }

            WaitForSingleObject( entryResult, unchecked(( uint )-1) );
            uint result = 0;
            GetExitCodeThread( entryResult, out result );
            Logger.Instance.WriteLine( $"result {result}" );

            Process noitaProcess = Process.GetProcessesByName("noita")[0];
            Logger.Instance.WriteLine( "noita.exe modules:" );
            foreach ( ProcessModule m in noitaProcess.Modules )
            {
                Logger.Instance.WriteLine( $"  {m.ModuleName}" );
            }

            CloseHandle( processHandle );
            return true;
        }

        static Injector instance;

        public static Injector Instance
        {
            get
            {
                if ( instance == null )
                {
                    instance = new Injector();
                }

                return instance;
            }
        }
    }
}