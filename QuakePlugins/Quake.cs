using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace QuakeEnhancedServerAnnouncer
{
    public static class Quake
    {
        public class Cvar
        {
            public IntPtr Pointer { get; set; }

            public float GetFloatValue(int defaultValue)
            {
                return Adapters.GetCvarFloatValue(this.Pointer, defaultValue);
            }

            public string GetStringValue()
            {
                var ptr = Adapters.GetCvarStringValue(this.Pointer);
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        public delegate void SetupInteropDelegate(IntPtr adaptersStructurePointer);


        private static class Adapters
        {
            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void AdapterPrintConsoleFn([MarshalAs(UnmanagedType.LPStr)] string text, uint color);
            public static AdapterPrintConsoleFn PrintConsole;

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate IntPtr AdapterRegisterCvarFn([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string description, [MarshalAs(UnmanagedType.LPStr)] string defaultValue,int flags, float min, float max);
            public static AdapterRegisterCvarFn RegisterCvar;

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate float GetCvarFloatValueFn(IntPtr cvar, int defaultValue);
            public static GetCvarFloatValueFn GetCvarFloatValue;

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate IntPtr GetCvarStringValueFn(IntPtr cvar);
            public static GetCvarStringValueFn GetCvarStringValue;

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void StartServerGameFn();
            public static StartServerGameFn StartServerGame;

            [UnmanagedFunctionPointer(CallingConvention.StdCall)]
            public delegate void ChangeGameFn([MarshalAs(UnmanagedType.LPStr)] string game);
            public static ChangeGameFn ChangeGame;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AdaptersStructure
        {
            public IntPtr PrintConsole;
            public IntPtr RegisterCvar;
            public IntPtr GetCvarFloatValue;
            public IntPtr GetCvarStringValue;
            public IntPtr StartServerGame;
            public IntPtr ChangeGame;
        };

        public static void SetupInterop(IntPtr adaptersPointer)
        {
            var adapters = Marshal.PtrToStructure<AdaptersStructure>(adaptersPointer);

            Adapters.PrintConsole = Marshal.GetDelegateForFunctionPointer<Adapters.AdapterPrintConsoleFn>(adapters.PrintConsole);
            Adapters.RegisterCvar = Marshal.GetDelegateForFunctionPointer<Adapters.AdapterRegisterCvarFn>(adapters.RegisterCvar);
            Adapters.GetCvarFloatValue = Marshal.GetDelegateForFunctionPointer<Adapters.GetCvarFloatValueFn>(adapters.GetCvarFloatValue);
            Adapters.GetCvarStringValue = Marshal.GetDelegateForFunctionPointer<Adapters.GetCvarStringValueFn>(adapters.GetCvarStringValue);
            Adapters.StartServerGame = Marshal.GetDelegateForFunctionPointer<Adapters.StartServerGameFn>(adapters.StartServerGame);
            Adapters.ChangeGame = Marshal.GetDelegateForFunctionPointer<Adapters.ChangeGameFn>(adapters.ChangeGame);
        }

        public static void StartServerGame()
        {
            Adapters.StartServerGame();
        }

        public static Cvar RegisterCvar(string name, string description, string defaultValue, int flags, float min, float max)
        {
            var ptr = Adapters.RegisterCvar(name, description, defaultValue, flags, min, max);

            return new Cvar() { Pointer = ptr };
        }

        public static void ChangeGame(string game)
        {
            Adapters.ChangeGame(game);
        }

        public static void PrintConsole(string text) => PrintConsole(text, (uint)Color.FromArgb(255, 224, 224, 224).ToArgb());
        public static void PrintConsole(string text, Color color) => PrintConsole(text, (uint)(color.A << 24 | color.B << 16 | color.G << 8 | color.R));
        public static void PrintConsole(string text, uint color)
        {
            Adapters.PrintConsole(text, color);
        }
    }
}
