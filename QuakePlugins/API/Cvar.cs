using QuakePlugins.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    /// <apitype />
    public class Cvar
    {
        private IntPtr _pointer;

        internal Cvar(IntPtr pointer)
        {
            _pointer = pointer;
        }
           
        /// <summary>
        /// Gets the string value of this cvar
        /// </summary>
        public string GetString()
        {
            return QEngine.CvarGetStringValue(_pointer);
        }

        /// <summary>
        /// Gets the numeric value of this cvar
        /// </summary>
        public float GetNumber()
        {
            return QEngine.CvarGetFloatValue(_pointer, 0);
        }

        /// <summary>
        /// Gets the boolean value of this cvar. If value >= 1 then it's considered true.
        /// </summary>
        /// <returns></returns>
        public bool GetBool()
        {
            return GetNumber() >= 1;
        }
    }
}
