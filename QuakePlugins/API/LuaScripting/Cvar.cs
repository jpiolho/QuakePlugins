using QuakePlugins.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API.LuaScripting
{
    internal class Cvar
    {
        private IntPtr _pointer;

        internal Cvar(IntPtr pointer)
        {
            _pointer = pointer;
        }

        public string GetString()
        {
            return QEngine.CvarGetStringValue(_pointer);
        }

        public float GetNumber()
        {
            return QEngine.CvarGetFloatValue(_pointer, 0);
        }

        public bool GetBool()
        {
            return GetNumber() >= 1;
        }
    }
}
