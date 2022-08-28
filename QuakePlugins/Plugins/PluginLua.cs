using NLua;
using NLua.Exceptions;
using QuakePlugins.API.LuaScripting;
using System;
using System.IO;
using static QuakePlugins.API.LuaScripting.Hooks;

namespace QuakePlugins.Plugins
{
    internal class PluginLua : Plugin
    {
        private Lua _state;

        protected override void OnInitialize()
        {
            InitializeLua();
            _state.DoFile(Path.Combine(FolderPath, "main.lua"));
        }


        private void InitializeLua()
        {
#pragma warning disable CS8974 // Converting method group to non-delegate type

            _state = new Lua();
            _state.HookException += LuaState_HookException;

            _state.LoadCLRPackage();

            _state.DoString(@"
luanet.load_assembly('System.Numerics.Vectors','System.Numerics')
Vector3 = luanet.import_type('System.Numerics.Vector3')

luanet.load_assembly('QuakePlugins','QuakePlugins.API')
Game = luanet.import_type('QuakePlugins.API.Game')
Console = luanet.import_type('QuakePlugins.API.Console')
Cvars = luanet.import_type('QuakePlugins.API.Cvars')
QC = luanet.import_type('QuakePlugins.API.QC')
Server = luanet.import_type('QuakePlugins.API.Server')
Builtins = luanet.import_type('QuakePlugins.API.Builtins')
Debug = luanet.import_type('QuakePlugins.API.Debug')
Client = luanet.import_type('QuakePlugins.API.Client')
");

            // Hooks
            _state.DoString("Hooks = {}");
            _state["Hooks.RegisterQC"] = (string name, LuaFunction func) => Hooks.RegisterQC(name, (args) => LuaReturnToHandling(func.Call(args)));
            _state["Hooks.DeregisterQC"] = Hooks.DeregisterQC;
            _state["Hooks.RegisterQCPost"] = (string name, LuaFunction func) => Hooks.RegisterQCPost(name, (args) => LuaReturnToHandling(func.Call(args)));
            _state["Hooks.DeregisterQCPost"] = Hooks.DeregisterQCPost;
            _state["Hooks.Register"] = (string name, LuaFunction func) => Hooks.Register(name, (args) => LuaReturnToHandling(func.Call(args)));
            _state["Hooks.Deregister"] = Hooks.Deregister;

            _state.DoString(@"
Hooks.Handling = luanet.import_type('QuakePlugins.API.LuaScripting.Hooks+Handling')
");

            _state.DoString("Timers = {}");
            _state["Timers.In"] = Timers.In;
            _state["Timers.At"] = Timers.At;
            _state["Timers.Stop"] = Timers.Stop;

#pragma warning restore CS8974 // Converting method group to non-delegate type
        }


        private Handling LuaReturnToHandling(object[] returns) => returns.Length > 0 ? (Handling)returns[0] : Handling.Continue;

        internal override Handling RaiseHook(string category, string name, params object[] args)
        {
            try
            {
                return base.RaiseHook(category, name, args);
            }
            catch(LuaScriptException ex)
            {
                Quake.PrintConsole($"[QuakePlugins] Unhandled Lua exception in addon '{Name}': {ex} | Line {ex.Source}\n", System.Drawing.Color.Red);
                return Handling.Continue;
            }
        }

        private void LuaState_HookException(object sender, NLua.Event.HookExceptionEventArgs e)
        {
            Quake.PrintConsole($"[QuakePlugins] Unhandled Lua exception in addon '{Name}': {e.Exception}\n", System.Drawing.Color.Red);
        }
    }
}
