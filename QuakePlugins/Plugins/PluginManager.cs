using QuakePlugins;
using QuakePlugins.API.LuaScripting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;

namespace QuakePlugins.Plugins
{
    public class PluginManager
    {
        private List<Plugin> _addons;

        public IReadOnlyList<Plugin> Addons => _addons.AsReadOnly();

        internal PluginManager()
        {
            _addons = new List<Plugin>();
        }

        internal void Start()
        {
            _addons.Clear();

            foreach (var folder in new DirectoryInfo(Path.Combine("rerelease","_addons")).GetDirectories())
            {
                // Skip disabled addons
                if (folder.Name.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase))
                    continue;

                Quake.PrintConsole($"Loading addon '{folder}'");

                Plugin addon = null;

                if(File.Exists(Path.Combine(folder.FullName,"main.lua")))
                {
                    Quake.PrintConsole(" (Lua)...\n");
                    addon = new PluginLua();
                }
                else
                {
                    Quake.PrintConsole(" (.NET)...\n");

                    var dll = Path.Combine(folder.FullName, "plugin.dll");
                    var loadContext = new PluginAssemblyLoadContext(dll);
                    var assembly = loadContext.LoadFromAssemblyPath(dll);

                    foreach(var type in assembly.GetTypes())
                    {
                        if(type.IsAssignableTo(typeof(Plugin)))
                        {
                            addon = (Plugin)Activator.CreateInstance(type);
                            break;
                        }
                    }
                }

                if(addon == null)
                {
                    Quake.PrintConsole(" (Unknown). Failed\n");
                    continue;
                }

                addon.Initialize(folder.Name, folder.FullName);

                _addons.Add(addon);
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
                    Quake.PrintConsole($"[QuakePlugins] Unhandled exception in addon '{addon.Name}': {ex}\n", Color.Red);
                }
            }

            return returnValue;
        }
    }
}
