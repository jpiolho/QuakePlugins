﻿using QuakePlugins.Core;
using QuakePlugins.Engine.Types;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions.Structs;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Memory.Sources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Void = Reloaded.Hooks.Definitions.Structs.Void;

namespace QuakePlugins.Engine
{
    internal class QEngine
    {
        internal static long func_enterFunc;
        internal static long func_printChat = 0; //0x1402a3a50;
        internal static long func_ed_loadFromFile = 0; //0x1401c9a30;
        internal static long func_getPlayfabGamemode = 0; //0x14037d420;
        internal static long hook_leaveFunc = 0; //0x1401cc024;
        internal static long var_executingFunction = 0; //0x1418c1160;
        internal static long func_r_newMap = 0; //0x1403221e0;

        private static IntPtr _pr_globals;
        private static IntPtr _pr_builtins;
        private static IntPtr _pr_builtin;
        private static unsafe int* _pr_argc;
        private static unsafe EngineEdict** _sv_edicts;
        private static unsafe uint* _pr_edict_size;
        private static unsafe char*** _g_gamedir;
        private static unsafe EngineServerStatic* _serverStatic;
        private static unsafe EngineQCFunction** _pr_functions;
        private static unsafe void* _client_worldmodel;
        private static unsafe EngineQCStatement** _pr_statements;
        private static unsafe EnginePlayfabClient** _g_playfabClients;
        private static unsafe long* _gTemporaryStringCounter;
        private static unsafe int* _gTemporaryStringMax;
        private static unsafe void** _gTemporaryStringBase;


        private static Stack<(byte[],int)> _stack;
        //private static int _qc_argcbackup;

        public static void InitializeQEngine()
        {
            _stack = new Stack<(byte[],int)>(); // new byte[112];


            func_enterFunc = Offsets.GetOffsetLong("PR_EnterFunction");


            var hooks = ReloadedHooks.Instance;
            _consolePrint = hooks.CreateWrapper<FnConsolePrint>(Offsets.GetOffsetLong("PrintConsole"), out _);
            _cvarRegister = hooks.CreateWrapper<FnCvarRegister>(Offsets.GetOffsetLong("CvarRegister"), out _);
            _cvarGet = hooks.CreateWrapper<FnCvarGet>(Offsets.GetOffsetLong("CvarGet"), out _);
            _qcFindFunction = hooks.CreateWrapper<FnQCFindFunction>(Offsets.GetOffsetLong("ED_FindFunction"), out _);
            _stringGet = hooks.CreateWrapper<FnStringGet>(Offsets.GetOffsetLong("GetPRString"), out _);
            _qcExecuteProgram = hooks.CreateWrapper<FnQCExecuteProgram>(Offsets.GetOffsetLong("PR_ExecuteProgram"), out _);
            _edictGetField = hooks.CreateWrapper<FnEdictGetField>(Offsets.GetOffsetLong("ED_FindField"), out _);
            _stringCreate = hooks.CreateWrapper<FnStringCreate>(Offsets.GetOffsetLong("CreateEngineString"), out _);

            /*
            _cvarGetFloatValue = hooks.CreateWrapper<FnCvarGetFloatValue>(0x1400dc770, out _);
            _gameGetGamemodeName = hooks.CreateWrapper<FnGameGetGamemodeName>(0x1401c6730, out _); // TODO: Fix
            */

            unsafe
            {
                _pr_functions = (EngineQCFunction**)Offsets.GetOffsetLong("pr_functions");
                _pr_builtins = Offsets.GetOffsetPointer("pr_builtins");
                _pr_builtin = new IntPtr(*(void**)_pr_builtins);
                _pr_globals = Offsets.GetOffsetPointer("pr_globals");
                _pr_edict_size = (uint*)Offsets.GetOffsetNativePointer("pr_edict_size");
                _pr_statements = (EngineQCStatement**)Offsets.GetOffsetNativePointer("pr_statements");
                _pr_argc = (int*)Offsets.GetOffsetNativePointer("pr_argc");
                _sv_edicts = (EngineEdict**)Offsets.GetOffsetNativePointer("sv_edicts");
                _serverStatic = (EngineServerStatic*)Offsets.GetOffsetNativePointer("svs");
                _g_playfabClients = (EnginePlayfabClient**)Offsets.GetOffsetNativePointer("gPlayfabClients");
                _gTemporaryStringCounter = (long*)Offsets.GetOffsetNativePointer("gTemporaryStringCounter");
                _gTemporaryStringMax = (int*)Offsets.GetOffsetNativePointer("gTemporaryStringMax");
                _gTemporaryStringBase = (void**)Offsets.GetOffsetNativePointer("gTemporaryStringBase");
                /*
                _g_gamedir = (char***)0x140e58b18;
                _client_worldmodel = (void*)0x149dcc438;
                */
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

        public static void QCSetStringTemporaryValue(QCValueOffset offset, string value)
        {
            QCSetIntValue(offset, StringCreateTemporary(value));
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
                hooks.CreateWrapper<FnBuiltIn>(*(long*)(_pr_builtins.ToInt64() + index * sizeof(void*)), out _).Value.Invoke(0);
            }
        }

        public static void QCRegistersBackup()
        {
            unsafe
            {
                var backup = new byte[112];
                Marshal.Copy(new IntPtr(*(EngineGlobalVars**)_pr_globals.ToPointer()), backup, 0, 28 * sizeof(int));

                _stack.Push((backup,*_pr_argc));
            }
        }

        public static void QCRegistersRestore()
        {
            unsafe
            {
                var backup = _stack.Pop();
                *_pr_argc = backup.Item2;
                Marshal.Copy(backup.Item1, 0, new IntPtr(*(EngineGlobalVars**)_pr_globals.ToPointer()), 28 * sizeof(int));
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
                *(void**)(Offsets.GetOffsetLong("CvarList") + 24) = null; // Rebuild cvars

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
                    return _cvarGet.Value.Invoke(new IntPtr(*(long*)Offsets.GetOffsetLong("CvarLast")), ptr);
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
                    _consolePrint.Value.Invoke(new IntPtr(Offsets.GetOffsetLong("PrintConsoleBuffer")), new IntPtr(&color), textPointer);
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
                return Marshal.PtrToStringUTF8(str);
            }
        }


        [Function(CallingConventions.Microsoft)]
        private struct FnEdictGetField { public FuncPtr<IntPtr, IntPtr> Value; }
        private static FnEdictGetField _edictGetField;

        public static unsafe EngineField* EdictGetField(string field)
        {
            unsafe
            {
                var ptr = Utils.MarshalStringToHGlobalUTF8(field);

                try
                {
                    return (EngineField*)_edictGetField.Value.Invoke(ptr).ToPointer();
                }
                finally
                {
                    Marshal.FreeHGlobal(ptr);
                }
            }
        }


        [Function(CallingConventions.Microsoft)]
        private struct FnEnterFunction { public FuncPtr<IntPtr, int> Value; }
        private static FnEnterFunction _enterFunction;

        [Function(CallingConventions.Microsoft)]
        private struct FnQCExecuteProgram { public FuncPtr<int, Void> Value; }
        private static FnQCExecuteProgram _qcExecuteProgram;

        public static unsafe void QCCallFunction(EngineQCFunction* function)
        {
            QCRegistersBackup();

            var debug1 = new IntPtr(_pr_functions);
            var debug2 = *(long*)_pr_functions;
            var debug3 = new IntPtr(function);

            var functionIndex = QCFunctionPointerToIndex(function);
            _qcExecuteProgram.Value.Invoke(functionIndex);

            QCRegistersRestore();
        }

        public static unsafe int QCFunctionPointerToIndex(EngineQCFunction* function)
        {
            return (int)(((long)function - *(long*)_pr_functions) / sizeof(EngineQCFunction));
        }

        [Function(CallingConventions.Microsoft)]
        private struct FnQCFindFunction { public FuncPtr<IntPtr, IntPtr> Value; }
        private static FnQCFindFunction _qcFindFunction;
        public static unsafe EngineQCFunction* QCGetFunctionByName(string name)
        {
            var str = Utils.MarshalStringToHGlobalUTF8(name);

            try
            {
                var debug1 = _qcFindFunction.Value.Invoke(str);
                return (EngineQCFunction*)debug1.ToPointer();
            }
            finally
            {
                Marshal.FreeHGlobal(str);
            }
        }


        public static unsafe EngineQCFunction* QCGetFunction(int functionNumber)
        {
            return (EngineQCFunction*)(*_pr_functions + (functionNumber * sizeof(EngineQCFunction)));
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
                return Marshal.PtrToStringUTF8(new IntPtr(*_g_gamedir));
            }
        }


        public static int BuiltinsGetOriginalCount()
        {
            return (int)((Offsets.GetOffsetLong("pr_builtin_end") - Offsets.GetOffsetLong("pr_builtin")) / IntPtr.Size);
        }

        public static IntPtr[] BuiltinsGetOriginal()
        {
            unsafe
            {
                var builtins = new List<IntPtr>();

                var ptr = (void**)Offsets.GetOffsetNativePointer("pr_builtin");
                while (ptr < Offsets.GetOffsetNativePointer("pr_builtin_end"))
                    builtins.Add(new IntPtr(*ptr++));

                return builtins.ToArray();
            }
        }

        public static void BuiltinsSetPointer(IntPtr ptr)
        {
            var memory = new Memory();
            unsafe
            {
                var pr_builtins = Offsets.GetOffset("pr_builtins");
                memory.ChangePermission(pr_builtins, IntPtr.Size, Reloaded.Memory.Kernel32.Kernel32.MEM_PROTECTION.PAGE_READWRITE);
                memory.Write(pr_builtins, ptr);
                memory.ChangePermission(pr_builtins, IntPtr.Size, Reloaded.Memory.Kernel32.Kernel32.MEM_PROTECTION.PAGE_READONLY);
            }
        }


        public static int StringCreateTemporary(string text)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            unsafe
            {
                var tempId = *(_gTemporaryStringCounter)++;
                tempId %= (*_gTemporaryStringMax) + 1;

                var bytes = Encoding.UTF8.GetBytes(text);
                var ptr = new IntPtr((char*)*_gTemporaryStringBase + (0x400 * tempId));
                Marshal.Copy(bytes, 0, ptr, Math.Min(bytes.Length,0x400));

                return _stringCreate.Value.Invoke(ptr, ptr);
            }
        }



        [Function(CallingConventions.Microsoft)]
        private struct FnGameGetGamemodeName { public FuncPtr<IntPtr> Value; }
        private static FnGameGetGamemodeName _gameGetGamemodeName;
        public static string GameGetGamemodeName()
        {
            unsafe
            {
                return Marshal.PtrToStringUTF8(_gameGetGamemodeName.Value.Invoke());
            }
        }

        public static string GameCustomGamemodeName { get; set; }

        public static unsafe EngineServerStatic* ServerStatic => _serverStatic;


        public static unsafe void* ClientWorldModel => _client_worldmodel;

        public static unsafe EngineQCStatement* QCGetStatement(int index)
        {
            return &(*_pr_statements)[index];
        }


        public static unsafe List<IntPtr> GetPlayfabClients()
        {
            var list = new List<IntPtr>();
            var client = *_g_playfabClients;
            var nullClient = client;
            
            client = client->ptr1;
            while (client != nullClient)
            {
                list.Add(new IntPtr(client));

                var var2 = client->ptr3;
                if(!var2->bool2)
                {
                    var var1 = var2->ptr1->bool2;
                    client = var2;
                    var2 = var2->ptr1;

                    while(!var1)
                    {
                        var1 = var2->ptr1->bool2;
                        client = var2;
                        var2 = var2->ptr1;
                    }
                }
                else
                {
                    var var1 = client->ptr2->bool2;
                    var var4 = client->ptr2;
                    var2 = client;

                    client = var4;
                    while(!var1 && var2 == client->ptr3)
                    {
                        var1 = client->ptr2->bool2;
                        var4 = client->ptr2;
                        var2 = client;
                        client = var4;
                    }
                }
            }

            return list;
        }
    }
}
