using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Engine.Types
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct EngineQCFunction
    {
        public int firstStatement;
        public int parametersStart;
        public int locals;
        public int profile;
        public int name;
        public int file;
        public int numberOfParameters;
        public fixed byte parametersSize[8];
    }
}
