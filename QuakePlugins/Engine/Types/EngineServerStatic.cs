using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Engine.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct EngineServerStatic
    {
        public int maxclients;
        public int maxclientslimit;
        public IntPtr clients;
        public int serverflags;
        public int changelevelIssued;
    }
}
