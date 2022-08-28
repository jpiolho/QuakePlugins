using QuakePlugins.API.LuaScripting;
using QuakePlugins.Core;
using QuakePlugins.Engine;
using Reloaded.Hooks;
using Reloaded.Hooks.Definitions;
using Reloaded.Hooks.Definitions.X64;
using Reloaded.Hooks.Tools;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace QuakePlugins
{
    public static class Quake
    {

        [Function(CallingConventions.Microsoft)]
        public delegate void PR_LeaveFunction();
        private static IAsmHook _pr_leaveFunctionHook;
        private static IReverseWrapper<PR_LeaveFunction> _pr_leaveFunctionWrapper;

        [Function(CallingConventions.Microsoft)]
        public delegate int PR_EnterFunction(IntPtr function);
        private static IHook<PR_EnterFunction> _hook_pr_enterFunctionHook;
        
        [Function(CallingConventions.Microsoft)]
        public delegate void ED_LoadFromFile(IntPtr name);
        private static IHook<ED_LoadFromFile> _hook_ed_loadFromFile;
        
        [Function(CallingConventions.Microsoft)]
        public delegate void PrintChat(IntPtr unknown, int nameType, char messageType, IntPtr name, IntPtr message);
        private static IHook<PrintChat> _hook_printChat;

        [Function(CallingConventions.Microsoft)]
        public delegate IntPtr GetPlayfabGameModeName();
        internal static IHook<GetPlayfabGameModeName> _hook_getPlayfabGameModeName;

        [Function(CallingConventions.Microsoft)]
        public delegate void R_NewMap();
        internal static IHook<R_NewMap> _hook_r_newMap;

        [Function(CallingConventions.Microsoft)]
        public delegate void SV_SpawnServer(IntPtr parameter);
        internal static IHook<SV_SpawnServer> _hook_sv_spawnServer;
        


        private static unsafe int Hook_QC_StartFunction(IntPtr function)
        {
            int nameIndex = *(int*)(function + 16);

            var functionName = QEngine.StringGet(nameIndex);

            if(functionName == "StartFrame")
            {
                foreach (var addon in Program._addonsManager.Addons)
                {
                    try
                    {
                        addon.TimerTick();
                    }
                    catch (Exception ex)
                    {
                        Quake.PrintConsole("Exception: " + ex.ToString() + "\n", System.Drawing.Color.Red);
                    }

                    try
                    {
                        if (addon.RaiseHook(Hooks.HookEvent,"OnStartFrame") != API.LuaScripting.Hooks.Handling.Continue)
                            break;
                    }
                    catch (Exception ex)
                    {
                        Quake.PrintConsole("Exception: " + ex.ToString() + "\n", System.Drawing.Color.Red);
                    }
                }
            }


            var overrideStatementIndex = ExecuteQCHook(functionName);
            
            var originalStatementIndex = _hook_pr_enterFunctionHook.OriginalFunction(function);
            
            if(overrideStatementIndex.HasValue)
                return overrideStatementIndex.Value - 1;

            return originalStatementIndex;
        }

        private static unsafe void Hook_QC_EndFunction()
        {
            IntPtr function = new IntPtr(*(int**)QEngine.var_executingFunction);
            int nameIndex = *(int*)(function + 16);

            var functionName = QEngine.StringGet(nameIndex);

            ExecuteQCHookPost(functionName);
        }

        private static unsafe void OnLoadEdictsFromFile(IntPtr ptr)
        {

            try
            {
                QEngine.GameCustomGamemodeName = null;
                Program._addonsManager.Start();
            }
            catch (Exception ex)
            {
                Quake.PrintConsole("Exception: " + ex.ToString() + "\n", System.Drawing.Color.Red);
            }

            _hook_ed_loadFromFile.OriginalFunction(ptr);

            Program._addonsManager.RaiseHook(Hooks.HookEvent, "OnAfterEntitiesLoaded");
        }


        private static unsafe void OnPrintChat(IntPtr unknown, int nameType, char messageType, IntPtr name, IntPtr message)
        {
            /*
                This hook isn't ideal since it's hooking into the PrintChat function, but it's an easy way to get it to work
                with both playfab and listen server
            */

            var clrName = Marshal.PtrToStringAnsi(name);
            var clrMessage = Marshal.PtrToStringAnsi(message);

            foreach (var addon in Program._addonsManager.Addons)
            {
                try
                {
                    if (addon.RaiseHook(Hooks.HookEvent,"OnChat", clrName, clrMessage, messageType == 1) != Hooks.Handling.Continue)
                        break;
                }
                catch (Exception ex)
                {
                    Quake.PrintConsole($"[ADDON] Exception: {ex}\n", Color.Red);
                }
            }

            _hook_printChat.OriginalFunction(unknown, nameType, messageType, name, message);
        }


        private static unsafe void OnClientNewMap()
        {
            Program._addonsManager.RaiseHook(Hooks.HookEvent, "OnClientNewMap");
            
            _hook_r_newMap.OriginalFunction();
        }
        private static unsafe void OnSV_SpawnServer(IntPtr arg1)
        {
            try
            {
                Program._addonsManager.Start();
            }
            catch(Exception ex)
            {
                Quake.PrintConsole($"QuakePlugins unhandled exception: {ex}\n", Color.Red);
            }

            _hook_sv_spawnServer.OriginalFunction(arg1);
        }

        private static IntPtr _customGamemodeNamePtr;
        private static string _customGamemodeName;
        private static unsafe IntPtr OnGetPlayfabGameModeName()
        {
            if(_customGamemodeName != QEngine.GameCustomGamemodeName)
            {
                if (_customGamemodeNamePtr != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(_customGamemodeNamePtr);
                    _customGamemodeNamePtr = IntPtr.Zero;
                }

                _customGamemodeName = QEngine.GameCustomGamemodeName;
                
                if (!string.IsNullOrEmpty(_customGamemodeName))
                    _customGamemodeNamePtr = Utils.MarshalStringToHGlobalUTF8(_customGamemodeName);
            }

            if (_customGamemodeNamePtr != IntPtr.Zero)
                return _customGamemodeNamePtr;

            return _hook_getPlayfabGameModeName.OriginalFunction();
        }



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


        public static unsafe void SetupHooks()
        {
            /*
            _hook_pr_enterFunctionHook = ReloadedHooks.Instance.CreateHook<PR_EnterFunction>(Hook_QC_StartFunction, QEngine.func_enterFunc).Activate();

            _pr_leaveFunctionHook = new AsmHook(new string[]
            {
                $"use64",
                //$"sub rsp,8",
                PushAllx64,
                PushSseCallConvRegistersx64,
                $"{Utilities.GetAbsoluteCallMnemonics(Hook_QC_EndFunction,out _pr_leaveFunctionWrapper)}",
                PopSseCallConvRegistersx64,
                PopAllx64,
                //$"add rsp,8",
            }, (nuint)QEngine.hook_leaveFunc, Reloaded.Hooks.Definitions.Enums.AsmHookBehaviour.ExecuteAfter).Activate();
            */

            _hook_sv_spawnServer = ReloadedHooks.Instance.CreateHook<SV_SpawnServer>(OnSV_SpawnServer, Offsets.GetOffsetLong("SV_SpawnServer")).Activate();
            /*
            _hook_printChat = ReloadedHooks.Instance.CreateHook<PrintChat>(OnPrintChat, QEngine.func_printChat ).Activate();
            _hook_ed_loadFromFile = ReloadedHooks.Instance.CreateHook<ED_LoadFromFile>(OnLoadEdictsFromFile, QEngine.func_ed_loadFromFile).Activate();
            _hook_getPlayfabGameModeName = ReloadedHooks.Instance.CreateHook<GetPlayfabGameModeName>(OnGetPlayfabGameModeName, QEngine.func_getPlayfabGamemode).Activate();
            _hook_r_newMap = ReloadedHooks.Instance.CreateHook<R_NewMap>(OnClientNewMap, QEngine.func_r_newMap).Activate();
            */
        }


        public static void PrintConsole(string text) => PrintConsole(text, (uint)Color.FromArgb(255, 224, 224, 224).ToArgb());
        public static void PrintConsole(string text, Color color) => PrintConsole(text, (uint)(color.A << 24 | color.B << 16 | color.G << 8 | color.R));
        public static void PrintConsole(string text, uint color)
        {
            QEngine.ConsolePrint(text, color);
        }


        private static int? ExecuteQCHook(string name,params object[] args)
        {
            var result = Program._addonsManager.RaiseHook(Hooks.HookQC, name);

            if (result == Hooks.Handling.Superceded)
                return EngineUtils.QCGetReturnStatementIndex();

            return null;
        }

        private static int? ExecuteQCHookPost(string name, params object[] args)
        {
            var result = Program._addonsManager.RaiseHook(Hooks.HookQCPost, name);

            return null;
        }
    }
}
