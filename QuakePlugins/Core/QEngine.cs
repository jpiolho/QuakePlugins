using Reloaded.Hooks;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X64;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Void = Reloaded.Hooks.Definitions.Structs.Void;

namespace QuakePlugins.Core
{
    internal class QEngine
    {
        private static unsafe float* _pr_globals;

        public static void InitializeQEngine()
        {
            var hooks = ReloadedHooks.Instance;
            _consolePrint = hooks.CreateWrapper<FnConsolePrint>(0x1400d69a0,out _);
            _cvarRegister = hooks.CreateWrapper<FnCvarRegister>(0x1400da2c0, out _);
            _cvarGetFloatValue = hooks.CreateWrapper<FnCvarGetFloatValue>(0x1400dac50, out _);
            _cvarGet = hooks.CreateWrapper<FnCvarGet>(0x1400d9250, out _);

            unsafe
            {
                _pr_globals = *(float**)0x1418a2a00;
            }
        }

        public enum QCValueOffset
        {
            Return = 1,
            Parameter0 = 4,
            Parameter1 = 7,
            Parameter2 = 10,
            Parameter3 = 13,
            Parameter4 = 16,
            Parameter5 = 19,
            Parameter6 = 22,
            Parameter7 = 25
        }

        private int QCGetIntValue(QCValueOffset offset)
        {
            unsafe 
            { 
                return *(int*)&_pr_globals[(int)offset];
            }
        }

        private float QCGetFloatValue(QCValueOffset offset)
        {
            unsafe
            {
                return _pr_globals[(int)offset];
            }
        }

        private Vector3 QCGetVectorValue(QCValueOffset offset)
        {
            unsafe
            {
                float x = _pr_globals[(int)offset];
                float y = _pr_globals[(int)offset+1];
                float z = _pr_globals[(int)offset+2];
                return new Vector3(x, y, z);
            }
        }

        

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct KexArray_t
        {
            void* data;
            int capacity;
            int length;
            int unknown1;
            int unknown2;
        };

        [StructLayout(LayoutKind.Sequential)]
        private unsafe struct Cvar_t
        {
            char* name;
            char* description;
            char* defaultValue;
            int flags;

            fixed char _padding[4 + 4 + 4 + 8];

            Cvar_t* previous;
            Cvar_t* next;

            fixed char _padding2[8];

            void* callback;
            void* getString;

            fixed char _padding3[8];

            KexArray_t array1;
            KexArray_t array2;
        };

        [Function(CallingConventions.Microsoft)]
        private struct FnCvarRegister { public FuncPtr<IntPtr, IntPtr, IntPtr, IntPtr, int, float, float, bool, IntPtr, IntPtr> Value; }
        private static FnCvarRegister _cvarRegister;

        public static IntPtr CvarRegister(string name,string description,string defaultValue,int flags,float min,float max,bool unknown, IntPtr callback)
        {
            IntPtr namePointer = Marshal.StringToHGlobalAnsi(name);
            IntPtr descriptionPointer = Marshal.StringToHGlobalAnsi(description);
            IntPtr defaultValuePointer = Marshal.StringToHGlobalAnsi(defaultValue);


            unsafe
            {
                var cvarPtr = Marshal.AllocHGlobal(sizeof(Cvar_t));
                
                _cvarRegister.Value.Invoke(cvarPtr, namePointer, defaultValuePointer, descriptionPointer, flags, min, max, unknown, IntPtr.Zero);
                *(void**)(0x149e0c178 + 24) = null; // Rebuild cvars

                return cvarPtr;
            }


        }

        [Function(CallingConventions.Microsoft)]
        private struct FnCvarGet { public FuncPtr<IntPtr,IntPtr,IntPtr> Value; }
        private static FnCvarGet _cvarGet;
        public static IntPtr CvarGet(string name)
        {
            var ptr = Marshal.StringToHGlobalAnsi(name);

            try
            {
                unsafe
                {
                    return _cvarGet.Value.Invoke(new IntPtr(*(long*)0x140f9dcc0), ptr);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(ptr);
            }
        }

        public static string CvarGetStringValue(IntPtr cvar)
        {
            unsafe
            {
                return Marshal.PtrToStringAnsi(new IntPtr(**(char***)(cvar.ToInt64() + 96)));
            }
        }

        [Function(CallingConventions.Microsoft)]
        private struct FnCvarGetFloatValue { public FuncPtr<IntPtr, int, float> Value; }
        private static FnCvarGetFloatValue _cvarGetFloatValue;
        public static float CvarGetFloatValue(IntPtr cvar,int defaultValue)
        {
            unsafe
            {
                return _cvarGetFloatValue.Value.Invoke(cvar, defaultValue);
            }
        }

        [Function(CallingConventions.Microsoft)]
        private struct FnConsolePrint { public FuncPtr<IntPtr, IntPtr, IntPtr, Void> Value; }
        private static FnConsolePrint _consolePrint;
        public static void ConsolePrint(string text,uint color)
        {
            IntPtr textPointer = Marshal.StringToHGlobalAnsi(text);

            try
            {
                unsafe
                {
                    _consolePrint.Value.Invoke(new IntPtr(0x1409bf140), new IntPtr(&color), textPointer);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(textPointer);
            }
        }
    }
}
