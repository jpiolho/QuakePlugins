using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Engine.Types
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct EnginePlayfabClient
    {
        [FieldOffset(0)] public EnginePlayfabClient* ptr1;
        [FieldOffset(8)] public EnginePlayfabClient* ptr2;
        [FieldOffset(16)] public EnginePlayfabClient* ptr3;
        [FieldOffset(25)] public bool bool2;

        [FieldOffset(80)] public IntPtr name;
        [FieldOffset(88)] public long nameLength;
        [FieldOffset(32)] public IntPtr uniqueId;
        [FieldOffset(128)] public IntPtr networkId;
        [FieldOffset(136)] public long networkIdLength;
    }
}
