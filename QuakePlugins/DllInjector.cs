using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace QuakePlugins
{
    public static class DllInjector
    {
        private static class Interop
        {
            [Flags]
            public enum ThreadAccess : int
            {
                TERMINATE = (0x0001),
                SUSPEND_RESUME = (0x0002),
                GET_CONTEXT = (0x0008),
                SET_CONTEXT = (0x0010),
                SET_INFORMATION = (0x0020),
                QUERY_INFORMATION = (0x0040),
                SET_THREAD_TOKEN = (0x0080),
                IMPERSONATE = (0x0100),
                DIRECT_IMPERSONATION = (0x0200)
            }

            [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Ansi)]
            public static extern IntPtr LoadLibrary([MarshalAs(UnmanagedType.LPStr)] string lpFileName);


            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

            [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
            public static extern IntPtr GetModuleHandle(string lpModuleName);

            [DllImport("kernel32", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
            public static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
            public static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, uint dwSize, uint flAllocationType, uint flProtect);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, uint nSize, out UIntPtr lpNumberOfBytesWritten);
            [DllImport("kernel32.dll")]
            public static extern IntPtr CreateRemoteThread(IntPtr process, IntPtr lpThreadAttributes, uint dwStackSize, IntPtr lpStartAddress, IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern UInt32 WaitForSingleObject(IntPtr hHandle, UInt32 dwMilliseconds);

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool CloseHandle(IntPtr hObject);

            [DllImport("kernel32.dll")]
            public static extern uint GetLastError();

            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern int SuspendThread(IntPtr hThread);
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern uint ResumeThread(IntPtr hThread);
            [DllImport("kernel32.dll")]
            public static extern IntPtr OpenThread(ThreadAccess dwDesiredAccess, bool bInheritHandle, uint dwThreadId);

            // IntPtr output
            [DllImport("kernel32.dll", SetLastError = true)]
            public static extern bool GetExitCodeThread(IntPtr hThread, out IntPtr lpExitCode);

            public const int PROCESS_CREATE_THREAD = 0x0002;
            public const int PROCESS_QUERY_INFORMATION = 0x0400;
            public const int PROCESS_VM_OPERATION = 0x0008;
            public const int PROCESS_VM_WRITE = 0x0020;
            public const int PROCESS_VM_READ = 0x0010;

            public const uint MEM_COMMIT = 0x00001000;
            public const uint MEM_RESERVE = 0x00002000;
            public const uint PAGE_READWRITE = 4;
        }

        private static ProcessModule FindModuleByName(ProcessModuleCollection collection, string name)
        {
            foreach (ProcessModule module in collection)
                if (module.ModuleName.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return module;

            return null;
        }

        public static void Inject(Process process, string dllPath, string functionCall)
        {
            var dllName = Path.GetFileName(dllPath);
            var module = Interop.LoadLibrary(dllPath);
            if (module == IntPtr.Zero)
            {
                var err = Interop.GetLastError();
                throw new Exception($"Failed to load {dllName} module (Error: {err})");
            }

            // geting the handle of the process - with required privileges
            IntPtr procHandle = Interop.OpenProcess(Interop.PROCESS_CREATE_THREAD | Interop.PROCESS_QUERY_INFORMATION | Interop.PROCESS_VM_OPERATION | Interop.PROCESS_VM_WRITE | Interop.PROCESS_VM_READ, false, process.Id);

            // searching for the address of LoadLibraryA and storing it in a pointer
            IntPtr loadLibraryAddr = Interop.GetProcAddress(Interop.GetModuleHandle("kernel32.dll"), "LoadLibraryA");


            
            IntPtr LoadRemoteLibrary(string libraryPath)
            {
                // alocating some memory on the target process - enough to store the name of the dll
                // and storing its address in a pointer
                IntPtr allocMemAddress = Interop.VirtualAllocEx(procHandle, IntPtr.Zero, (uint)((libraryPath.Length + 1) * Marshal.SizeOf(typeof(char))), Interop.MEM_COMMIT | Interop.MEM_RESERVE, Interop.PAGE_READWRITE);

                // writing the name of the dll there
                UIntPtr bytesWritten;
                Interop.WriteProcessMemory(procHandle, allocMemAddress, Encoding.Default.GetBytes(libraryPath), (uint)((libraryPath.Length + 1) * Marshal.SizeOf(typeof(char))), out bytesWritten);

                // creating a thread that will call LoadLibraryA with allocMemAddress as argument
                var remoteThread = Interop.CreateRemoteThread(procHandle, IntPtr.Zero, 0, loadLibraryAddr, allocMemAddress, 0, IntPtr.Zero);

                var waitResult = Interop.WaitForSingleObject(remoteThread, 5000);
                if (waitResult != 0)
                    throw new Exception("Failed to inject dll");

                Interop.GetExitCodeThread(remoteThread, out var handleInjected);
                Interop.CloseHandle(remoteThread);

                return allocMemAddress;
            }
            

            LoadRemoteLibrary(Path.Combine(Path.GetDirectoryName(dllPath), "comhost.dll"));
            LoadRemoteLibrary(Path.Combine(Path.GetDirectoryName(dllPath), "ijwhost.dll"));
            LoadRemoteLibrary(Path.Combine(Path.GetDirectoryName(dllPath), "nethost.dll"));


            
            var allocMemAddress = LoadRemoteLibrary(dllPath);




            IntPtr ownAddrLoadNetCore = Interop.GetProcAddress(module, functionCall);
            var addrOffset = ownAddrLoadNetCore.ToInt64() - module.ToInt64();

            var remoteAddrLoadNetCore = new IntPtr(FindModuleByName(process.Modules, dllName).BaseAddress.ToInt64() + addrOffset);


            foreach (ProcessThread thread in process.Threads)
            {
                IntPtr pOpenThread = Interop.OpenThread(Interop.ThreadAccess.SUSPEND_RESUME, false, (uint)thread.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                Interop.SuspendThread(pOpenThread);

                Interop.CloseHandle(pOpenThread);
            }


            var remoteThread = Interop.CreateRemoteThread(procHandle, IntPtr.Zero, 0, remoteAddrLoadNetCore, allocMemAddress, 0, IntPtr.Zero);

            var waitResult = Interop.WaitForSingleObject(remoteThread, 5000);
                        
            var error = Interop.GetLastError();


            foreach (ProcessThread pT in process.Threads)
            {
                IntPtr pOpenThread = Interop.OpenThread(Interop.ThreadAccess.SUSPEND_RESUME, false, (uint)pT.Id);

                if (pOpenThread == IntPtr.Zero)
                {
                    continue;
                }

                var suspendCount = 0;
                do
                {
                    suspendCount = (int)Interop.ResumeThread(pOpenThread);
                } while (suspendCount > 0);

                Interop.CloseHandle(pOpenThread);
            }

        }
    }
}
