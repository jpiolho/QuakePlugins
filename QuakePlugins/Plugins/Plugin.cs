using QuakePlugins.API;
using QuakePlugins.API.LuaScripting;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace QuakePlugins.Plugins
{
    public abstract class Plugin
    {
        /// <summary>
        /// Full path to the plugin folder
        /// </summary>
        public string FolderPath { get; private set; }
        /// <summary>
        /// The name of the plugin
        /// </summary>
        public string Name { get; private set; }


        protected Hooks Hooks { get; private set; }
        protected Timers Timers { get; private set; }
        public Builtins Builtins { get; private set; }

        internal void Initialize(string name, string path)
        {
            Name = name;
            FolderPath = path;

            Hooks = new Hooks();
            Timers = new Timers();
            Builtins = new Builtins();

            OnInitialize();
        }

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


        internal virtual Hooks.Handling RaiseHook(string category, string name, params object[] args)
        {
            return Hooks.RaiseHooks(category, name, args);
        }

        internal void TimerTick()
        {
            Timers.Tick();
        }
    }
}
