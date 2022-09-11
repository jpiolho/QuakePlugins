using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Plugins.Runtimes
{
    internal class PluginDotnet : Plugin
    {
        public PluginAssemblyLoadContext LoadContext { get; private set; }
        public Plugin Plugin { get; private set; }

        public PluginDotnet(PluginAssemblyLoadContext loadContext,Plugin plugin)
        {
            LoadContext = loadContext;
            Plugin = plugin;
        }

        public override void OnRuntimeDestroy()
        {
            LoadContext.Unload();
        }
    }
}
