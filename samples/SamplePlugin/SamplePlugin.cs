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

        public void OnLoad( ILogger logger )
        {
            logger.WriteLine( "SamplePlugin.OnLoad()" );
            logger.WriteLine( "Hello from sample plugin :)" );
        }

        public void OnUnload()
        {

        }
    }
}
