using NLua;
using QuakePlugins.API.LuaScripting;
using System;
using System.Numerics;
using Console = QuakePlugins.API.Console;

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
");
            /*
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
            _state["QC.GetFloat"] = QC.GetFloat;
            _state["QC.GetInt"] = QC.GetInt;
            _state["QC.GetEdict"] = QC.GetEdict;
            _state["QC.GetVector"] = QC.GetVector;
            _state["QC.GetString"] = QC.GetString;
            _state["QC.SetFloat"] = QC.SetFloat;
            _state["QC.SetInt"] = QC.SetInt;
            _state["QC.SetString"] = QC.SetString;
            _state["QC.SetVector"] = QC.SetVector;
            _state["QC.SetEdict"] = QC.SetEdict;
            _state["QC.GetSelf"] = () => QC.Self;
            _state["QC.SetSelf"] = (Edict e) => QC.Self = e;
            _state["QC.GetOther"] = () => QC.Other;
            _state["QC.SetOther"] = (Edict e) => QC.Other = e;
            _state["QC.GetWorld"] = () => QC.World;
            _state["QC.GetTime"] = () => QC.Time;
            _state["QC.GetMsgEntity"] = () => QC.MsgEntity;
            _state["QC.SetMsgEntity"] = (Edict e) => QC.MsgEntity = e;
            _state["QC.CallFunction"] = QC.Call;

            // Game

            
            //_state["Game"] = typeof(Game);
            //_state["Game.Mod"] = () => Game.Mod;


            _state.DoString("Server = {}");
            _state["Server.GetClient"] = Server.GetClient;

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
            _state["Builtins.WriteByte"] = Builtins.WriteByte;
            _state["Builtins.WriteChar"] = Builtins.WriteChar;
            _state["Builtins.WriteCoord"] = Builtins.WriteCoord;
            _state["Builtins.WriteEntity"] = Builtins.WriteEntity;
            _state["Builtins.WriteLong"] = Builtins.WriteLong;
            _state["Builtins.WriteShort"] = Builtins.WriteShort;
            _state["Builtins.WriteString"] = Builtins.WriteString;
            _state["Builtins.NextEnt"] = Builtins.NextEnt;

            _state["Builtins.ByIndexVoid"] = Builtins.ByIndexVoid;
            _state["Builtins.ByIndexFloat"] = Builtins.ByIndexFloat;
            _state["Builtins.ByIndexBool"] = Builtins.ByIndexBool;
            _state["Builtins.ByIndexString"] = Builtins.ByIndexString;
            _state["Builtins.ByIndexVector"] = Builtins.ByIndexVector;
            _state["Builtins.ByIndexEdict"] = Builtins.ByIndexEdict;
            _state["Builtins.ByIndexInt"] = Builtins.ByIndexInt;
            */
            // Hooks
            _state.DoString("Hooks = {}");
            _state["Hooks.RegisterQC"] = _hooks.RegisterQC;
            _state["Hooks.DeregisterQC"] = _hooks.DeregisterQC;
            _state["Hooks.RegisterQCPost"] = _hooks.RegisterQCPost;
            _state["Hooks.DeregisterQCPost"] = _hooks.DeregisterQCPost;
            _state["Hooks.Register"] = _hooks.Register;
            _state["Hooks.Deregister"] = _hooks.Deregister;


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

        public void RaiseQCHookPost(string name)
        {
            if (!_hooks.QCHooksPost.TryGetValue(name, out var hookedFunctions))
                return;

            foreach (var hook in hookedFunctions)
                hook.Call();
        }

        public void RaiseEvent(string eventName, params object[] args)
        {
            if (!_hooks.EventHooks.TryGetValue(eventName, out var hookedFunctions))
                return;

            foreach (var hook in hookedFunctions)
                hook.Call(args);
        }

        public void Dispose()
        {
            _hooks.QCHooks.Clear();
            _hooks.QCHooksPost.Clear();
            _state.Dispose();
        }
    }
}
