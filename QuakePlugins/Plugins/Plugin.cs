using QuakePlugins.API;
using QuakePlugins.API.LuaScripting;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuakePlugins.Plugins
{
    public abstract class Plugin
    {
        public string FolderPath { get; private set; }
        public PluginInfo Info { get; private set; }

        protected Hooks Hooks { get; private set; }
        protected Timers Timers { get; private set; }
        public Builtins Builtins { get; private set; }


        internal void Initialize(string path,PluginInfo info)
        {
            FolderPath = path;
            Info = info;

            Hooks = new Hooks();
            Timers = new Timers();
            Builtins = new Builtins();

            OnInitialize();
        }

        public virtual void OnRuntimeInitialize() { }
        public virtual void OnRuntimeDestroy() { }

        protected virtual void OnInitialize() { }
        protected virtual void OnLoad() { }
        protected virtual void OnUnload() { }
        protected virtual void OnDestroy() { }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RaiseOnLoad() => OnLoad();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RaiseOnUnload() => OnUnload();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void RaiseOnDestroy() => OnDestroy();


        public virtual Hooks.Handling RaiseHook(string category, string name, params object[] args)
        {
            return Hooks.RaiseHooks(category, name, args);
        }

        internal void TimerTick()
        {
            Timers.Tick();
        }
    }
}
