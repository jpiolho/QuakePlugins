using NLua;
using QuakePlugins;
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


        internal void RaiseQCHook(string name)
        {
            _lua.RaiseQCHook(name);
            // TODO: C# Raise QC Hook
        }

        internal void RaiseQCHookPost(string name)
        {
            _lua.RaiseQCHookPost(name);
            // TODO: C# Raise QC Hook
        }

        internal void RaiseEvent(string eventName, params object[] args)
        {
            _lua.RaiseEvent(eventName, args);
            // TODO: C# Raise Event
        }

        internal void TimerTick()
        {
            _lua.TimersTick();
        }
    }
}
