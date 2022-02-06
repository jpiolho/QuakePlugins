using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins
{
    internal class Utils
    {
        public static IntPtr MarshalStringToHGlobalUTF8(string text)
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            var ptr = Marshal.AllocHGlobal(bytes.Length);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);

            return ptr;
        }
    }
}
