using System.Runtime.InteropServices;

namespace QuakePlugins.Engine.Types
{
    [StructLayout(LayoutKind.Sequential)]
    public struct EngineVector3
    {
        public float X, Y, Z;
    }
}
