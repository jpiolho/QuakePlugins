using QuakePlugins.Plugins;
using QuakePlugins.Plugins.Runtimes;

namespace QuakePlugins.LuaRuntime
{
    [PluginRuntime(Id ="lua")]
    public class QuakePluginsLuaRuntime : IPluginRuntime
    {
        public Task<Plugin> LoadAsync(string path, PluginInfo info)
        {
            return Task.FromResult<Plugin>(new PluginLua());
        }
    }
}