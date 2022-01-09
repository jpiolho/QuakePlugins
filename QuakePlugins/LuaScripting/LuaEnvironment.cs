using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.LuaScripting
{
    internal class LuaEnvironment
    {
        private Lua _state;
        public Lua LuaState => _state;
        public bool IsInitialized { get; private set; }

        public LuaEnvironment() { }


        public event EventHandler<Exception> LuaException;

        public void Initialize()
        {
            _state = new Lua();
            _state.HookException += LuaState_OnHookException;

            // Console
            _state.DoString("Console = {}");
            _state["Console.Print"] = (Action<string, uint?>)API.LuaScripting.Console.Print;

            // Cvars
            _state.DoString("Cvars = {}");
            _state["Cvars.Register"] = (Action<string, string, string>)API.LuaScripting.Cvars.Register;
        }

        private void LuaState_OnHookException(object sender, NLua.Event.HookExceptionEventArgs e)
        {
            LuaException?.Invoke(this, e.Exception);
        }

        public void ExecuteFile(string file)
        {
            _state.DoFile(file);
        }
    }
}
