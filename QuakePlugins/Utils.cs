using QuakePlugins.API;
using QuakePlugins.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
            var ptr = Marshal.AllocHGlobal(bytes.Length+1);
            Marshal.Copy(bytes, 0, ptr, bytes.Length);
            
            // Set zero terminator
            unsafe
            {
                *(byte*)(ptr.ToInt64() + bytes.Length) = 0;
            }

            return ptr;
        }
    }
}
