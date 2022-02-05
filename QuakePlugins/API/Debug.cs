using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.API
{
    /// <apiglobal />
    public static class Debug
    {
        /// <summary>
        /// Triggers a debugger breakpoint.
        /// </summary>
        public static void Break(string breakpointName,params object[] args)
        {
            Debugger.Break();
        }
    }
}
