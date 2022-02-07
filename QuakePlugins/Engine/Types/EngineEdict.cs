using System.Runtime.InteropServices;

namespace QuakePlugins.Engine.Types
{
    [StructLayout(LayoutKind.Explicit)]
    internal struct EngineEdict
    {
        [FieldOffset(216)] public EngineEntityVars vars;
    }
}
