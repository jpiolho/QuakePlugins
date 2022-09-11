using QuakePlugins.Plugins.Runtimes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Plugins.Runtimes
{
    public interface IPluginRuntime
    {
        Task<Plugin> LoadAsync(string path,PluginInfo info);
    }
}
