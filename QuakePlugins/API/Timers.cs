using NLua;
using QuakePlugins.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    /// <apiglobal />
    public class Timers
    {
        private List<(float time, LuaFunction func, object[] args)?> _luaFunctions;

        internal Timers()
        {
            _luaFunctions = new List<(float time,LuaFunction func, object[] args)?>();
        }

        /// <summary>
        /// Sets a function to be called in a specified amount of game time.
        /// </summary>
        public int In(float time, LuaFunction func, params object[] args)
        {
            unsafe
            {
                return AddTimer((GetTriggerTime(time), func, args));
            }
        }

        /// <summary>
        /// Sets a function to be called at a specific game time.
        /// </summary>
        public int At(float time, LuaFunction func, params object[] args)
        {
            return AddTimer((time, func, args));
        }

        /// <summary>
        /// Stops a timer
        /// </summary>
        public void Stop(int id)
        {
            if(id > 0 && id < _luaFunctions.Count)
                _luaFunctions[id] = null;
        }

        private int AddTimer((float,LuaFunction,object[]) timer)
        {
            for(var i=0;i<_luaFunctions.Count;i++)
            {
                if(!_luaFunctions[i].HasValue)
                {
                    _luaFunctions[i] = timer;
                    return i;
                }
            }

            _luaFunctions.Add(timer);
            return _luaFunctions.Count - 1;
        }

        internal void Tick()
        {
            unsafe
            {
                float currentTime = QEngine.GetGlobals()->time;

                for (int i = 0; i < _luaFunctions.Count; i++)
                {
                    var timer = _luaFunctions[i];
                    if (timer.HasValue && currentTime >= timer.Value.time)
                    {
                        timer.Value.func.Call(timer.Value.args);
                        _luaFunctions[i] = null;
                    }
                        
                }
            }
        }

        private unsafe float GetTriggerTime(float relativeTime)
        {
            return QEngine.GetGlobals()->time + relativeTime;
        }
    }
}
