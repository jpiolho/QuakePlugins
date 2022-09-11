using QuakePlugins;
using QuakePlugins.API;
using QuakePlugins.API.LuaScripting;
using QuakePlugins.Plugins.Runtimes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

namespace QuakePlugins.Plugins
{
    public class PluginManager
    {
        private static JsonSerializerOptions jsonSerializerOptions = new JsonSerializerOptions()
        {
            ReadCommentHandling = JsonCommentHandling.Skip,
            PropertyNameCaseInsensitive = true
        };

        private List<Plugin> _addons;

        public IReadOnlyList<Plugin> Addons => _addons.AsReadOnly();

        private Dictionary<string,IPluginRuntime> _runtimes;

        internal PluginManager()
        {
            _addons = new List<Plugin>();
            _runtimes = new Dictionary<string, IPluginRuntime>();
        }

        internal void Start()
        {
            // TODO: Unload plugins & runtimes
            _addons.Clear();
            _runtimes.Clear();

            List<(string folder, PluginInfo info)> availablePlugins = new();

            // Get a list of all plugins & parse their info
            foreach (var folder in new DirectoryInfo(Path.Combine("rerelease", "_addons")).GetDirectories())
            {
                // Skip disabled addons
                if (folder.Name.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase))
                    continue;

                string raw;
                try
                {
                    raw = File.ReadAllText(Path.Combine(folder.FullName, "plugin.json"));
                }
                catch (FileNotFoundException)
                {
                    continue;
                }

                var json = JsonSerializer.Deserialize<PluginInfo>(raw, jsonSerializerOptions);
                availablePlugins.Add((folder.FullName, json));
            }

            _runtimes["dotnet"] = new PluginRuntimeDotnet();
            
            foreach(var plugin in availablePlugins.OrderBy(p => p.info.Type.Equals(PluginTypes.Runtime,StringComparison.OrdinalIgnoreCase) ? 0 : 100))
            {
                if (plugin.info.Type.Equals(PluginTypes.Runtime, StringComparison.OrdinalIgnoreCase))
                {
                    QConsole.PrintLine($"Loading runtime '{plugin.info.Name}'");

                    var dll = Path.Combine(plugin.folder, plugin.info.Main);
                    var loadContext = new PluginAssemblyLoadContext(dll);
                    var assembly = loadContext.LoadFromAssemblyPath(dll);

                    foreach(var type in assembly.GetTypes())
                    {
                        var runtimeAttribute = type.GetCustomAttribute<PluginRuntimeAttribute>();
                        if (runtimeAttribute != null)
                        {
                            if (!type.IsAssignableTo(typeof(IPluginRuntime)))
                                throw new Exception($"Type '{type}' is marked as a PluginRuntime but does not extend IPluginRuntime");

                            var runtimeId = runtimeAttribute.Id.ToLowerInvariant();
                            if (_runtimes.ContainsKey(runtimeId))
                                throw new Exception("A runtime with the id '{}' already exists");

                            var pluginRuntime = (IPluginRuntime)Activator.CreateInstance(type);
                            _runtimes.Add(runtimeId, pluginRuntime);

                            QConsole.PrintLine($"Registered runtime '{runtimeId}'");
                        }
                    }
                }
                else
                {
                    var folder = plugin.folder;
                    QConsole.PrintLine($"Loading plugin '{folder}'");

                    var runtimeId = plugin.info.Runtime.ToLowerInvariant();
                    if (!_runtimes.TryGetValue(runtimeId, out var runtime))
                        throw new Exception($"Could not find runtime '{runtimeId}'");

                    var loadedPlugin = runtime.LoadAsync(plugin.folder, plugin.info).GetAwaiter().GetResult();

                    loadedPlugin.OnRuntimeInitialize();
                    loadedPlugin.Initialize(plugin.folder,plugin.info);

                    _addons.Add(loadedPlugin);
                }
            }

            foreach(var addon in _addons)
            {
                addon.RaiseOnLoad();
            }
        }

        
        internal Hooks.Handling RaiseHook(string category,string name,params object[] args)
        {
            Hooks.Handling returnValue = Hooks.Handling.Continue;

            foreach(var addon in _addons)
            {
                try
                {
                    returnValue = addon.RaiseHook(category, name, args);

                    if (returnValue != Hooks.Handling.Continue)
                        return returnValue;
                }
                catch(Exception ex)
                {
                    Quake.PrintConsole($"[QuakePlugins] Unhandled exception in addon '{addon.Info.Name}': {ex}\n", Color.Red);
                }
            }

            return returnValue;
        }
    }
}
