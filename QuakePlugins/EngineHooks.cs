using QuakePlugins;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins
{
    internal static class EngineHooks
    {
        public delegate void OnQCFunctionCallDelegate(string name);

        public static void OnQCFunctionCallBefore(string name) {
            Quake.PrintConsole("QC function before: " + name + "\n");
        }

        public static void OnQCFunctionCallAfter(string name)
        {
            Quake.PrintConsole("QC function after: " + name + "\n");
        }
    }
}
