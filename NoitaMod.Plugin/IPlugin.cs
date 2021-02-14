using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoitaMod.Plugin
{
    public struct PluginInfo
    {
        public string Name;
    }

    public interface IPlugin
    {
        PluginInfo PluginInfo { get; set; }

        void OnLoad();

        void OnUnload();
    }
}
