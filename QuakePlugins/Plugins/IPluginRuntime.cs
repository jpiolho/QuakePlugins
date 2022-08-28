using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Plugins
{
    internal interface IPluginRuntime
    {
        internal event EventHandler<Exception> UnhandledException;

        internal void Load();
    }
}
