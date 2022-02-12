using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API.LuaScripting
{
    /// <apiglobal />
    public class Hooks
    {
        public enum Handling
        {
            Continue = 0,
            Handled = 1,
            Superceded = 2
        }


        private Dictionary<string,List<LuaFunction>> _qcHooks;
        private Dictionary<string,List<LuaFunction>> _qcHooksPost;
        private Dictionary<string, List<LuaFunction>> _eventHooks;

        public Dictionary<string, List<LuaFunction>> QCHooks => _qcHooks;
        public Dictionary<string, List<LuaFunction>> QCHooksPost => _qcHooksPost;
        public Dictionary<string, List<LuaFunction>> EventHooks => _eventHooks;

        public Hooks()
        {
            _qcHooks = new Dictionary<string, List<LuaFunction>>();
            _qcHooksPost = new Dictionary<string, List<LuaFunction>>();
            _eventHooks = new Dictionary<string, List<LuaFunction>>();
        }

        public void RegisterQC(string name,LuaFunction func)
        {
            if (!_qcHooks.TryGetValue(name, out var hookList))
                _qcHooks[name] = hookList = new List<LuaFunction>();

            hookList.Add(func);
        }

        public bool DeregisterQC(string name,LuaFunction func)
        {
            if (!_qcHooks.TryGetValue(name, out var hookList))
                return false;

            return hookList.Remove(func);
        }

        public void RegisterQCPost(string name, LuaFunction func)
        {
            if (!_qcHooksPost.TryGetValue(name, out var hookList))
                _qcHooksPost[name] = hookList = new List<LuaFunction>();

            hookList.Add(func);
        }

        public bool DeregisterQCPost(string name,LuaFunction func)
        {
            if (!_qcHooksPost.TryGetValue(name, out var hookList))
                return false;

            return hookList.Remove(func);
        }

        public void Register(string name, LuaFunction func)
        {
            if (!_eventHooks.TryGetValue(name, out var hookList))
                _eventHooks[name] = hookList = new List<LuaFunction>();

            hookList.Add(func);
        }

        public bool Deregister(string name, LuaFunction func)
        {
            if (!_eventHooks.TryGetValue(name, out var hookList))
                return false;

            return hookList.Remove(func);
        }


        internal Handling RaiseHooks(Dictionary<string,List<LuaFunction>> hookList,string name,params object[] arguments)
        {
            if (!hookList.TryGetValue(name, out var hookedFunctions))
                return Handling.Continue;

            foreach (var hook in hookedFunctions)
            {
                var returns = hook.Call(arguments);

                if (returns.Length > 0 && (Handling)returns[0] != Handling.Continue)
                    return (Handling)returns[0];
            }

            return Handling.Continue;
        }
    }
}
