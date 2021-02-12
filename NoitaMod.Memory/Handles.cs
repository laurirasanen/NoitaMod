using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoitaMod.Log;

namespace NoitaMod.Memory
{
    public class Handles
    {
        public static IntPtr Process;

        public static void Init( IntPtr process )
        {
            Logger.Instance.WriteLine( "NoitaMod.Memory.Handles.Init()" );

            Process = process;
        }
    }
}
