using System;
using System.Collections.Generic;
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

        public static uint Read( IntPtr address )
        {
            byte[] dataBuffer = new byte[4];
            int bytesRead = 0;
            ReadProcessMemory( ( int )Handles.Process, ( int )address, dataBuffer, dataBuffer.Length, ref bytesRead );
            return BitConverter.ToUInt32( dataBuffer, 0 );
        }

        public static bool Write( IntPtr address, uint value )
        {
            byte[] dataBuffer = BitConverter.GetBytes(value);
            int bytesWritten = 0;
            WriteProcessMemory( ( int )Handles.Process, ( int )address, dataBuffer, dataBuffer.Length, ref bytesWritten );
            return true;
        }
    }
}
