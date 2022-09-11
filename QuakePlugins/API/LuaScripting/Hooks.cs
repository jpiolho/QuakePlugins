using System.Collections.Generic;

namespace QuakePlugins.API.LuaScripting
{
    /// <apiglobal />
    public class Hooks
    {
        internal const string HookEvent = "Event";
        internal const string HookQC = "QC";
        internal const string HookQCPost = "QCPost";

        public enum Handling
        {
            Continue = 0,
            Handled = 1,
            Superceded = 2
        }

        public delegate Handling HookCallback(object[] args);

        private Dictionary<string, List<HookCallback>> _hooks;


        internal Hooks()
        {
            _hooks = new Dictionary<string, List<HookCallback>>();
        }


        private string GetHookID(string category, string name) => category + "|" + name;

        private bool RegisterHook(string category, string name, HookCallback func)
        {
            var hookId = GetHookID(category, name);

            // Get or create the hook list for the category
            if (!_hooks.TryGetValue(hookId, out var hookList))
                _hooks[hookId] = hookList = new List<HookCallback>();

            // Prevent duplicate hooks
            if (!hookList.Contains(func))
            {
                hookList.Add(func);
                return true;
            }

            return false;
        }

        private bool DeregisterHook(string category, string name, HookCallback func)
        {
            var hookId = GetHookID(category, name);

            if (!_hooks.TryGetValue(hookId, out var hookList))
                return false;

            return hookList.Remove(func);
        }

        public void RegisterQC(string name, HookCallback func) => RegisterHook("QC", name, func);
        public bool DeregisterQC(string name, HookCallback func) => DeregisterHook("QC", name, func);

        public void RegisterQCPost(string name, HookCallback func) => RegisterHook("QCPost", name, func);
        public bool DeregisterQCPost(string name, HookCallback func) => DeregisterHook("QCPost", name, func);

        public void Register(string name, HookCallback func) => RegisterHook("Event", name, func);
        public bool Deregister(string name, HookCallback func) => DeregisterHook("Event", name, func);


        internal Handling RaiseHooks(string category, string name, params object[] arguments)
        {
            var hookId = GetHookID(category, name);
            if (!_hooks.TryGetValue(hookId, out var hookList))
                return Handling.Continue;

            foreach (var hook in hookList)
            {
                var returnValue = hook.Invoke(arguments);

                if (returnValue != Handling.Continue)
                    return returnValue;
            }

            return Handling.Continue;
        }
    }
}
