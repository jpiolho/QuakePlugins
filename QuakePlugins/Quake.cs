using QuakePlugins.Core;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Hooks.Tools;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace QuakePlugins
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
            public delegate IntPtr AdapterRegisterCvarFn([MarshalAs(UnmanagedType.LPStr)] string name, [MarshalAs(UnmanagedType.LPStr)] string description, [MarshalAs(UnmanagedType.LPStr)] string defaultValue, int flags, float min, float max);
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


        [Function(CallingConventions.Microsoft)]
        public delegate int PR_EnterFunction(IntPtr function);
        [Function(CallingConventions.Microsoft)]
        public delegate void PR_LeaveFunction();

        private static unsafe int MyHook(IntPtr function)
        {
            int nameIndex = *(int*)(function + 16);

            char** pr_strings = *(char***)(0x141a4a600);
            char* name = (char*)((long)pr_strings + nameIndex);

            string functionName = Marshal.PtrToStringAnsi(new IntPtr(name));

            //Quake.PrintConsole("QC Enter Function: " + functionName + "\n");
            /*
            char** pr_strings = *((char***)0x141a4a600);
            char* name = (char*)((long long)pr_strings + nameIndex);
            */

            foreach (var addon in Program._addonsManager?.Addons)
            {
                try
                {
                    addon.RaiseQCHook(functionName);
                }
                catch(Exception ex)
                {
                    Quake.PrintConsole($"[ADDON] Exception: {ex}\n", Color.Red);
                }
            }

            return _pr_enterFunctionHook.OriginalFunction(function);
        }

        private static unsafe void MyHook2()
        {


            IntPtr function = new IntPtr(*(int*)0x1418a2a40);

            int nameIndex = *(int*)(function + 16);

            char** pr_strings = *(char***)(0x141a4a600);
            char* name = (char*)((long)pr_strings + nameIndex);

            string functionName = Marshal.PtrToStringAnsi(new IntPtr(name));

            //Quake.PrintConsole("QC Leave Function: " + functionName + "\n");

            ;
            //Quake.PrintConsole("QC Leave hooked\n");
        }

        private static IHook<PR_EnterFunction> _pr_enterFunctionHook;
        private static IAsmHook _pr_leaveFunctionHook;
        private static IReverseWrapper<PR_LeaveFunction> _pr_leaveFunctionWrapper;

        public const string PushAllx64 = "push rax\n" +
                                     "push rbx\n" +
                                     "push rcx\n" +
                                     "push rdx\n" +
                                     "push rsi\n" +
                                     "push rdi\n" +
                                     "push rbp\n" +
                                     "push rsp\n" +
                                     "push r8\n" +
                                     "push r9\n" +
                                     "push r10\n" +
                                     "push r11\n" +
                                     "push r12\n" +
                                     "push r13\n" +
                                     "push r14\n" +
                                     "push r15";

        public const string PopAllx64 = "pop r15\n" +
                                        "pop r14\n" +
                                        "pop r13\n" +
                                        "pop r12\n" +
                                        "pop r11\n" +
                                        "pop r10\n" +
                                        "pop r9\n" +
                                        "pop r8\n" +
                                        "pop rsp\n" +
                                        "pop rbp\n" +
                                        "pop rdi\n" +
                                        "pop rsi\n" +
                                        "pop rdx\n" +
                                        "pop rcx\n" +
                                        "pop rbx\n" +
                                        "pop rax";

        public static string PushSseCallConvRegistersx64 = "sub rsp, 128\n" +
                                                       "movdqu  dqword [rsp + 0], xmm0\n" +
                                                       "movdqu  dqword [rsp + 16], xmm1\n" +
                                                       "movdqu  dqword [rsp + 32], xmm2\n" +
                                                       "movdqu  dqword [rsp + 48], xmm3\n" +
                                                       "movdqu  dqword [rsp + 64], xmm4\n" +
                                                       "movdqu  dqword [rsp + 80], xmm5\n" +
                                                       "movdqu  dqword [rsp + 96], xmm6\n" +
                                                       "movdqu  dqword [rsp + 112], xmm7\n";

        public static string PopSseCallConvRegistersx64 = "movdqu  xmm0, dqword [rsp + 0]\n" +
                                                           "movdqu  xmm1, dqword [rsp + 16]\n" +
                                                           "movdqu  xmm2, dqword [rsp + 32]\n" +
                                                           "movdqu  xmm3, dqword [rsp + 48]\n" +
                                                           "movdqu  xmm4, dqword [rsp + 64]\n" +
                                                           "movdqu  xmm5, dqword [rsp + 80]\n" +
                                                           "movdqu  xmm6, dqword [rsp + 96]\n" +
                                                           "movdqu  xmm7, dqword [rsp + 112]\n" +
                                                           "add rsp, 128";

        public static unsafe void SetupInterop(IntPtr adaptersPointer)
        {
            Debugger.Launch();

            QEngine.InitializeQEngine();

            var adapters = Marshal.PtrToStructure<AdaptersStructure>(adaptersPointer);

            Adapters.GetCvarStringValue = Marshal.GetDelegateForFunctionPointer<Adapters.GetCvarStringValueFn>(adapters.GetCvarStringValue);
            Adapters.StartServerGame = Marshal.GetDelegateForFunctionPointer<Adapters.StartServerGameFn>(adapters.StartServerGame);
            Adapters.ChangeGame = Marshal.GetDelegateForFunctionPointer<Adapters.ChangeGameFn>(adapters.ChangeGame);



        }

        public static unsafe void SetupHooks()
        {
            _pr_enterFunctionHook = ReloadedHooks.Instance.CreateHook<PR_EnterFunction>(MyHook, 0x1401c7390).Activate();

            //Quake.PrintConsole("HOOK: " + Utilities.GetAbsoluteCallMnemonics(MyHook2, out _pr_leaveFunctionWrapper) + "\n");
            // 0x1401c7de8
            _pr_leaveFunctionHook = new AsmHook(new string[]
            {
                $"use64",
                //$"sub rsp,8",
                PushAllx64,
                PushSseCallConvRegistersx64,
                $"{Utilities.GetAbsoluteCallMnemonics(MyHook2,out _pr_leaveFunctionWrapper)}",
                PopSseCallConvRegistersx64,
                PopAllx64,
                //$"add rsp,8",
            }, 0x1401c7df0, Reloaded.Hooks.Definitions.Enums.AsmHookBehaviour.ExecuteFirst).Activate();


        }



        public static void StartServerGame()
        {
            Adapters.StartServerGame();
        }

        public static Cvar RegisterCvar(string name, string description, string defaultValue, int flags, float min, float max)
        {
            QEngine.CvarRegister(name, description, defaultValue, flags, min, max, false, IntPtr.Zero);

            return null;
        }

        public static void ChangeGame(string game)
        {
            Adapters.ChangeGame(game);
        }

        public static void PrintConsole(string text) => PrintConsole(text, (uint)Color.FromArgb(255, 224, 224, 224).ToArgb());
        public static void PrintConsole(string text, Color color) => PrintConsole(text, (uint)(color.A << 24 | color.B << 16 | color.G << 8 | color.R));
        public static void PrintConsole(string text, uint color)
        {
            QEngine.ConsolePrint(text, color);
        }
    }
}
