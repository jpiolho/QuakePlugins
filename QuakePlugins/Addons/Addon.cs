using NLua;
using QuakePlugins;
using QuakePlugins.API.LuaScripting;
using QuakePlugins.LuaScripting;
using System;
using System.IO;

namespace QuakePlugins.Addons
{
    public class Addon
    {
        private string _path;
        private LuaEnvironment _lua;

        internal Addon(string path)
        {
            _path = path;
        }

        internal void Load()
        {
            _lua = new LuaEnvironment();
            _lua.LuaException += Lua_Exception;
            _lua.Initialize();

            _lua.ExecuteFile(Path.Combine(_path, "main.lua"));
        }

        private void Lua_Exception(object sender, Exception e)
        {
            Quake.PrintConsole($"Lua Exception: {e}\n",System.Drawing.Color.Red);
        }


        internal Hooks.Handling RaiseQCHook(string name)
        {
            return _lua.RaiseQCHook(name);
            // TODO: C# Raise QC Hook
        }

        internal Hooks.Handling RaiseQCHookPost(string name)
        {
            return _lua.RaiseQCHookPost(name);
            // TODO: C# Raise QC Hook
        }

        internal Hooks.Handling RaiseEvent(string eventName, params object[] args)
        {
            return _lua.RaiseEvent(eventName, args);
            // TODO: C# Raise Event
        }

        internal void TimerTick()
        {
            _lua.TimersTick();
        }
    }
}
