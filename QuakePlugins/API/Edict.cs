using QuakePlugins.Core;
using QuakePlugins.Engine.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    public class Edict
    {
        private int _edictIndex;
        private unsafe EngineEdict* _edict;

        public int Index => _edictIndex;
        public unsafe EngineEdict* EngineEdict => _edict;

        internal unsafe Edict(int index,EngineEdict* pointer)
        {
            _edictIndex = index;
            _edict = pointer;
        }


        public string Classname
        {
            get {
                unsafe
                {
                    return QEngine.StringGet(_edict->vars.classname);
                }
            }

            set
            {
                unsafe
                {
                    _edict->vars.classname = QEngine.StringCreate(value);
                }
            }
        }


        public void SetField(string name,object value)
        {
            unsafe
            {
                EngineEntityVars.GetFieldByName(name)?.SetValue(_edict->vars, value);
            }
        }

        public object GetField(string name)
        {
            unsafe
            {
                return EngineEntityVars.GetFieldByName(name)?.GetValue(_edict->vars) ?? null;
            }
        }
    }
}
