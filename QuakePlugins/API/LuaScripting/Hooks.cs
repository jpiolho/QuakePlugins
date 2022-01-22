using NLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API.LuaScripting
{
    internal class Hooks
    {
        private Dictionary<string,List<LuaFunction>> _qcHooks;

        public Dictionary<string, List<LuaFunction>> QCHooks => _qcHooks;

        public Hooks()
        {
            _qcHooks = new Dictionary<string, List<LuaFunction>>();
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
    }
}
