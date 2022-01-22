using NLua;
using QuakePlugins.API;
using QuakePlugins.API.LuaScripting;
using System;
using Console = QuakePlugins.API.LuaScripting.Console;

namespace QuakePlugins.LuaScripting
{
    internal class LuaEnvironment
    {
        private Lua _state;
        public Lua LuaState => _state;
        public bool IsInitialized { get; private set; }

        public LuaEnvironment() { }


        public event EventHandler<Exception> LuaException;


        private Hooks _hooks;

        public void Initialize()
        {
            _state = new Lua();
            _state.HookException += LuaState_OnHookException;

            _hooks = new Hooks();

            // Console
            _state.DoString("Console = {}");
            _state["Console.Print"] = (Action<string, uint?>)Console.Print;

            // Cvars
            _state.DoString("Cvars = {}");
            _state["Cvars.Register"] = (Func<string, string, string, Cvar>)Cvars.Register;
            _state["Cvars.Get"] = (Func<string, Cvar>)Cvars.Get;

            _state.DoString("Hooks = {}");
            _state["Hooks.RegisterQC"] = (Action<string, LuaFunction>)_hooks.RegisterQC;
        }

        private void LuaState_OnHookException(object sender, NLua.Event.HookExceptionEventArgs e)
        {
            LuaException?.Invoke(this, e.Exception);
        }

        public void ExecuteFile(string file)
        {
            _state.DoFile(file);
        }


        public void RaiseQCHook(string name)
        {
            if (!_hooks.QCHooks.TryGetValue(name, out var hookedFunctions))
                return;

            foreach (var hook in hookedFunctions)
                hook.Call();
        }
    }
}
