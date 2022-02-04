﻿using QuakePlugins.Engine.Types;
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

namespace QuakePlugins.Engine
{
    internal class QEngine
    {
        private static IntPtr _pr_globals;
        private static IntPtr _pr_builtin;
        private static unsafe int* _pr_argc;
        private static unsafe EngineEdict** _sv_edicts;
        private static unsafe uint* _pr_edict_size;
        private static unsafe char*** _g_gamedir;
        private static unsafe EngineServerStatic* _serverStatic;
        private static unsafe void** _pr_functions;

        private static byte[] _stack;
        private static int _qc_argcbackup;

        public static void InitializeQEngine()
        {
            _stack = new byte[112];

            var hooks = ReloadedHooks.Instance;
            _consolePrint = hooks.CreateWrapper<FnConsolePrint>(0x1400d69a0, out _);
            _cvarRegister = hooks.CreateWrapper<FnCvarRegister>(0x1400da2c0, out _);
            _cvarGetFloatValue = hooks.CreateWrapper<FnCvarGetFloatValue>(0x1400dac50, out _);
            _cvarGet = hooks.CreateWrapper<FnCvarGet>(0x1400d9250, out _);
            _stringGet = hooks.CreateWrapper<FnStringGet>(0x1401c2550, out _);
            _stringCreate = hooks.CreateWrapper<FnStringCreate>(0x1401c25c0, out _);


            unsafe
            {
                _g_gamedir = (char***)0x140e4bb18;
                _pr_globals = new IntPtr(0x1418a2a00);
                _pr_builtin = new IntPtr(0x1409a5a80);
                _pr_argc = (int*)0x1418a2a38;
                _sv_edicts = (EngineEdict**)0x1418beeb0;
                _pr_edict_size = (uint*)0x1418a29f8;
                _serverStatic = (EngineServerStatic*)0x141a607d0;
                _pr_functions = (void**)0x1418a2a28;
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

        public static int QCGetIntValue(QCValueOffset offset)
        {
            unsafe
            {
                return (*(EngineGlobalVars**)_pr_globals.ToPointer())->qcRegisters[(int)offset];
            }
        }

        public static float QCGetFloatValue(QCValueOffset offset)
        {
            unsafe
            {
                return (*(EngineGlobalVars**)_pr_globals.ToPointer())->qcRegisters[(int)offset];
            }
        }

        public static Vector3 QCGetVectorValue(QCValueOffset offset)
        {
            unsafe
            {
                float x = (*(EngineGlobalVars**)_pr_globals.ToPointer())->qcRegisters[(int)offset];
                float y = (*(EngineGlobalVars**)_pr_globals.ToPointer())->qcRegisters[(int)offset + 1];
                float z = (*(EngineGlobalVars**)_pr_globals.ToPointer())->qcRegisters[(int)offset + 2];
                return new Vector3(x, y, z);
            }
        }

        public static string QCGetStringValue(QCValueOffset offset)
        {
            return StringGet(QCGetIntValue(offset));
        }

        public static unsafe EngineEdict* QCGetEdictValue(QCValueOffset offset)
        {
            return EdictGetByOffset(QCGetIntValue(offset));
        }

        public static void QCSetIntValue(QCValueOffset offset, int value)
        {
            unsafe
            {
                (*(EngineGlobalVars**)_pr_globals.ToPointer())->qcRegisters[(int)offset] = value;
            }
        }
        public static void QCSetFloatValue(QCValueOffset offset, float value)
        {
            unsafe
            {
                *(float*)&(*(EngineGlobalVars**)_pr_globals.ToPointer())->qcRegisters[(int)offset] = value;
            }
        }
        public static void QCSetStringValue(QCValueOffset offset, string value)
        {
            QCSetIntValue(offset, StringCreate(value));
        }
        public static void QCSetVectorValue(QCValueOffset offset, Vector3 value)
        {
            unsafe
            {
                *(float*)&(*(EngineGlobalVars**)_pr_globals.ToPointer())->qcRegisters[(int)offset] = value.X;
                *(float*)&(*(EngineGlobalVars**)_pr_globals.ToPointer())->qcRegisters[(int)offset + 1] = value.Y;
                *(float*)&(*(EngineGlobalVars**)_pr_globals.ToPointer())->qcRegisters[(int)offset + 2] = value.Z;
            }
        }
        public static unsafe void QCSetEdictValue(QCValueOffset offset, EngineEdict* edict)
        {
            QCSetIntValue(offset, EdictGetOffset(edict));
        }

        private struct FnBuiltIn { public FuncPtr<int, Void> Value; }
        public static void BuiltinCall(int index)
        {
            unsafe
            {
                var hooks = ReloadedHooks.Instance;
                hooks.CreateWrapper<FnBuiltIn>(*(long*)(_pr_builtin.ToInt64() + index * sizeof(void*)), out _).Value.Invoke(0);
            }
        }

        public static void QCRegistersBackup()
        {
            unsafe
            {
                _qc_argcbackup = *_pr_argc;
                Marshal.Copy(new IntPtr(*(EngineGlobalVars**)_pr_globals.ToPointer()), _stack, 0, 28 * sizeof(int));
            }
        }

        public static void QCRegistersRestore()
        {
            unsafe
            {
                *_pr_argc = _qc_argcbackup;
                Marshal.Copy(_stack, 0, new IntPtr(*(EngineGlobalVars**)_pr_globals.ToPointer()), 28 * sizeof(int));
            }
        }

        public static void QCSetArgumentCount(int count)
        {
            unsafe
            {
                *_pr_argc = count;
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

        public static IntPtr CvarRegister(string name, string description, string defaultValue, int flags, float min, float max, bool unknown, IntPtr callback)
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
        private struct FnCvarGet { public FuncPtr<IntPtr, IntPtr, IntPtr> Value; }
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
        public static float CvarGetFloatValue(IntPtr cvar, int defaultValue)
        {
            unsafe
            {
                return _cvarGetFloatValue.Value.Invoke(cvar, defaultValue);
            }
        }

        [Function(CallingConventions.Microsoft)]
        private struct FnConsolePrint { public FuncPtr<IntPtr, IntPtr, IntPtr, Void> Value; }
        private static FnConsolePrint _consolePrint;
        public static void ConsolePrint(string text, uint color)
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


        [Function(CallingConventions.Microsoft)]
        private struct FnStringGet { public FuncPtr<int, int, IntPtr> Value; }
        private static FnStringGet _stringGet;
        public static string StringGet(int index)
        {
            unsafe
            {
                if (index == 0)
                    return null;

                IntPtr str = _stringGet.Value.Invoke(0, index);
                return Marshal.PtrToStringAnsi(str);
            }
        }

        [Function(CallingConventions.Microsoft)]
        private struct FnEnterFunction { public FuncPtr<IntPtr, int> Value; }
        private static FnEnterFunction _enterFunction;

        public static unsafe void QCCallFunction(IntPtr function)
        {
            QCRegistersBackup();
            
            _enterFunction.Value.Invoke(function);

            QCRegistersRestore();
        }

        public static unsafe IntPtr QCGetFunctionByName(string name)
        {
            var ptr = new IntPtr(*_pr_functions);

            while(true)
            {
                var fnName = StringGet(*(int*)(ptr.ToInt64() + 16));

                if (fnName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return ptr;


                ptr += 1;
            }

        }


        public static unsafe IntPtr QCGetFunction(int functionNumber)
        {
            return new IntPtr(*_pr_functions) + functionNumber;
        }

        [Function(CallingConventions.Microsoft)]
        private struct FnStringCreate { public FuncPtr<IntPtr, IntPtr, int> Value; }
        private static FnStringCreate _stringCreate;
        public static int StringCreate(string str)
        {
            var ptr = Marshal.StringToHGlobalAnsi(str);
            unsafe
            {
                return _stringCreate.Value.Invoke(ptr + str.Length + 1, ptr);
            }
        }


        public static unsafe EngineGlobalVars* GetGlobals()
        {
            return *(EngineGlobalVars**)_pr_globals.ToPointer();
        }

        public static unsafe int EdictGetOffset(EngineEdict* edict)
        {
            return (int)((long)edict - (long)*_sv_edicts);
        }
        public static unsafe EngineEdict* EdictGetByOffset(int offset)
        {
            return (EngineEdict*)((long)*_sv_edicts + offset);
        }

        public static unsafe int EdictGetIndex(EngineEdict* edict)
        {
            return (int)(EdictGetOffset(edict) / *_pr_edict_size);
        }

        public static unsafe int EdictGetIndex(int offset)
        {
            return (int)(offset / *_pr_edict_size);
        }

        public static unsafe EngineEdict* EdictGetByNumber(int number)
        {
            unsafe
            {
                return (EngineEdict*)((ulong)*_sv_edicts + *_pr_edict_size * (uint)number);
            }
        }

        public static string GameGetGameDir()
        {
            unsafe
            {
                return Marshal.PtrToStringAuto(new IntPtr(*_g_gamedir));
            }
        }

        public static unsafe EngineServerStatic* ServerStatic => _serverStatic;
    }
}