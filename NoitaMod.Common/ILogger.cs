using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoitaMod.Common
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warn,
        Error
    }

    public interface ILogger
    {
        void WriteLine( string text, LogLevel level = LogLevel.Debug );
    }
}
