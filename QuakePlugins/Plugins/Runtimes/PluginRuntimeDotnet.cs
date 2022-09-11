using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Plugins.Runtimes
{
    internal class PluginRuntimeDotnet : IPluginRuntime
    {
        public Task<Plugin> LoadAsync(string path, PluginInfo info)
        {
            var dll = Path.Combine(path, info.Main);
            var loadContext = new PluginAssemblyLoadContext(dll);
            var assembly = loadContext.LoadFromAssemblyPath(dll);

            foreach (var type in assembly.GetTypes())
            {
                if (!type.IsAssignableTo(typeof(Plugin)))
                    continue;

                var plugin = (Plugin)Activator.CreateInstance(type);
                return Task.FromResult<Plugin>(plugin);
            }

            throw new Exception("Could not find Plugin class");
        }
    }
}
