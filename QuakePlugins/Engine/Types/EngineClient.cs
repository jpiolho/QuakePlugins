using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Engine.Types
{
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct EngineClient
    {
        public const int SizeOf = 82232;

        [FieldOffset(0)]
        public bool active;

        [FieldOffset(82008)]
        public EngineEdict* edict;

        [FieldOffset(82016)]
        public fixed char name[32];

        [FieldOffset(82052)]
        public int color;
    }
}
