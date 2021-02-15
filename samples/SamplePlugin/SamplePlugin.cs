using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NoitaMod.Common;

namespace SamplePlugin
{
    public class SamplePlugin : IPlugin
    {
        private PluginInfo pluginInfo;

        public PluginInfo PluginInfo
        {
            get { return pluginInfo; }
            set { pluginInfo = value; }
        }

        public SamplePlugin()
        {
            pluginInfo = new PluginInfo()
            {
                Name = "SamplePlugin"
            };
        }

        public void OnLoad( Host host )
        {
            host.Logger.WriteLine( "SamplePlugin.OnLoad()" );

            string pattern = "6c 69 67 68 74 6e 69 6e 67 5f 63 6f 75 6e 74 00 6d 61 74 65 72 69 61 6c";
            ulong addr = host.Scanner.FindPattern(pattern);

            if ( addr == 0 )
            {
                host.Logger.WriteLine( $"SamplePlugin: Could not find pattern {pattern}" );
            }
            else
            {
                host.Logger.WriteLine( $"SamplePlugin: Found pattern '{pattern}' at 00x{addr.ToString( "X8" )}" );
            }
        }

        public void OnUnload()
        {

        }
    }
}
