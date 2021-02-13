using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace NoitaMod.Memory
{
    public class Operations
    {
        // Externs
        [DllImport( "kernel32" )]
        public static extern bool ReadProcessMemory( int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int loNumberOfBytesRead );

        [DllImport( "kernel32", SetLastError = true )]
        public static extern bool WriteProcessMemory( int hProcess, int lpBaseAddress, byte[] lpBuffer, int dwSize, ref int loNumberOfBytesRead );

        public static int Read( ulong address, byte[] buffer, int size )
        {
            return Read( ( IntPtr )address, buffer, size );
        }

        public static int Read( IntPtr address, byte[] buffer, int size )
        {
            if ( size > buffer.Length )
            {
                throw new InvalidOperationException();
            }
            int bytesRead = 0;
            ReadProcessMemory( ( int )Handles.Process, ( int )address, buffer, size, ref bytesRead );
            return bytesRead;
        }

        public static int Write( IntPtr address, byte[] buffer, int size )
        {
            if ( size > buffer.Length )
            {
                throw new InvalidOperationException();
            }
            int bytesWritten = 0;
            WriteProcessMemory( ( int )Handles.Process, ( int )address, buffer, size, ref bytesWritten );
            return bytesWritten;
        }

        public static ProcessModuleCollection GetProcessModules()
        {
            return Process.GetCurrentProcess().Modules;
        }
    }
}
