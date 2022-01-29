using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Engine.Types
{
    [StructLayout(LayoutKind.Explicit)]
    public struct EngineEdict
    {
        [FieldOffset(216)] public EngineEntityVars vars;
    }
}
