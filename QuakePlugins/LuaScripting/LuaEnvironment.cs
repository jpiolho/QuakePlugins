using NLua;
using QuakePlugins.API;
using QuakePlugins.API.LuaScripting;
using System;
using System.Numerics;
using Console = QuakePlugins.API.LuaScripting.Console;

namespace QuakePlugins.LuaScripting
{
    internal class LuaEnvironment : IDisposable
    {
        private Lua _state;
        public Lua LuaState => _state;
        public bool IsInitialized { get; private set; }

        public LuaEnvironment() { }


        public event EventHandler<Exception> LuaException;


        private Hooks _hooks;

        public void Initialize()
        {
#pragma warning disable CS8974 // Converting method group to non-delegate type
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

            // QC
            _state.DoString("QC = {}");
            _state.DoString("QC.Value = {}");
            _state["QC.Value.Return"] = QC.ValueLocation.Return;
            _state["QC.Value.Parameter0"] = QC.ValueLocation.Parameter0;
            _state["QC.Value.Parameter1"] = QC.ValueLocation.Parameter1;
            _state["QC.Value.Parameter2"] = QC.ValueLocation.Parameter2;
            _state["QC.Value.Parameter3"] = QC.ValueLocation.Parameter3;
            _state["QC.Value.Parameter4"] = QC.ValueLocation.Parameter4;
            _state["QC.Value.Parameter5"] = QC.ValueLocation.Parameter5;
            _state["QC.Value.Parameter6"] = QC.ValueLocation.Parameter6;
            _state["QC.Value.Parameter7"] = QC.ValueLocation.Parameter7;
            _state["QC.GetFloat"] = (Func<QC.ValueLocation, float>)QC.GetFloat;
            _state["QC.GetInt"] = (Func<QC.ValueLocation, int>)QC.GetInt;
            _state["QC.GetEdict"] = (Func<QC.ValueLocation, Edict>)QC.GetEdict;
            _state["QC.GetVector"] = (Func<QC.ValueLocation, Vector3>)QC.GetVector;
            _state["QC.GetString"] = (Func<QC.ValueLocation, string>)QC.GetString;
            _state["QC.SetFloat"] = (Action<QC.ValueLocation,float>)QC.SetFloat;
            _state["QC.SetInt"] = (Action<QC.ValueLocation,int>)QC.SetInt;
            _state["QC.SetString"] = (Action<QC.ValueLocation,string>)QC.SetString;
            _state["QC.SetVector"] = (Action<QC.ValueLocation,Vector3>)QC.SetVector;
            _state["QC.SetEdict"] = (Action<QC.ValueLocation, Edict>)QC.SetEdict;

            // Builtins
            _state.DoString("Builtins = {}");
            _state["Builtins.Makevectors"] = Builtins.Makevectors;
            _state["Builtins.SetOrigin"] = Builtins.SetOrigin;
            _state["Builtins.SetModel"] = Builtins.SetModel;
            _state["Builtins.BPrint"] = Builtins.BPrint;
            _state["Builtins.SPrint"] = Builtins.SPrint;
            _state["Builtins.Stuffcmd"] = Builtins.Stuffcmd;
            _state["Builtins.Localcmd"] = Builtins.Localcmd;
            _state["Builtins.Spawn"] = Builtins.Spawn;
            _state["Builtins.LocalSound"] = Builtins.LocalSound;
            _state["Builtins.DrawPoint"] = Builtins.DrawPoint;
            _state["Builtins.DrawLine"] = Builtins.DrawLine;
            _state["Builtins.DrawArrow"] = Builtins.DrawArrow;
            _state["Builtins.DrawRay"] = Builtins.DrawRay;
            _state["Builtins.DrawCircle"] = Builtins.DrawCircle;
            _state["Builtins.DrawBounds"] = Builtins.DrawBounds;
            _state["Builtins.DrawWorldText"] = Builtins.DrawWorldText;
            _state["Builtins.DrawSphere"] = Builtins.DrawSphere;
            _state["Builtins.DrawCylinder"] = Builtins.DrawCylinder;
            _state["Builtins.PrecacheSound"] = Builtins.PrecacheSound;
            _state["Builtins.PrecacheModel"] = Builtins.PrecacheModel;

            _state.DoString("Hooks = {}");
            _state["Hooks.RegisterQC"] = (Action<string, LuaFunction>)_hooks.RegisterQC;
#pragma warning restore CS8974 // Converting method group to non-delegate type
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

        public void Dispose()
        {
            _hooks.QCHooks.Clear();
            _state.Dispose();
        }
    }
}
