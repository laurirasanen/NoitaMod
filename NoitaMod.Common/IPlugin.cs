using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoitaMod.Common
{
    public struct PluginInfo
    {
        public string Name;
    }

    public interface IPlugin
    {
        PluginInfo PluginInfo { get; set; }

        void OnLoad(ILogger logger);

        void OnUnload();
    }
}
