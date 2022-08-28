using QuakePlugins.Addons;
using QuakePlugins.API;
using QuakePlugins.Core;
using QuakePlugins.Engine;
using Reloaded.Injector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Console = QuakePlugins.API.Console;

namespace QuakePlugins
{
    public static class Program
    {
        public delegate void MainInjectedDelegate(IntPtr rootPtr);

        public static AddonsManager _addonsManager;

        static void MainInjected(IntPtr quakePluginsRootPtr)
        {
            var root = Marshal.PtrToStringAuto(quakePluginsRootPtr);

            //Debugger.Launch();

            try
            {
                Offsets.LoadAsync(root).Wait();

                QEngine.InitializeQEngine();

                Quake.PrintConsole("QuakePlugins loaded\n", System.Drawing.Color.Green);

                _addonsManager = new AddonsManager();

                Quake.SetupHooks();
            }
            catch(Exception ex)
            {
                Quake.PrintConsole("Exception: " + ex.ToString() + "\n", System.Drawing.Color.Red);
            }
            
        }

        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // Try resolving it to a loaded assembly
            var loadedAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(a => a.FullName == args.Name);
            if (loadedAssembly != null)
                return loadedAssembly;

            // Resolve it to an assembly in the same folder
            var assemblyFile = args.Name.Split(",")[0] + ".dll";
            var path = Path.Combine(Path.GetDirectoryName(args.RequestingAssembly.Location), assemblyFile);
            if (File.Exists(path))
                return Assembly.LoadFrom(path);


            return null;
        }




        [StructLayout(LayoutKind.Sequential)]
        unsafe struct DotnetInitializeParameters
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
            public byte[] DllPath;
        }

        static async Task Main(string[] args)
        {
            System.Console.WriteLine("Loading offsets...");
            await Offsets.LoadAsync();

            System.Console.WriteLine("Searching for quake process...");
            var processes = Process.GetProcessesByName("Quake_x64_steam");

            if (processes.Length < 1)
                throw new Exception("Could not find process");

            if (processes.Length > 1)
                throw new Exception("Too many processes found");

            using var injector = new Injector(processes[0]);
            var addr1 = injector.Inject(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\6.0.7\runtimes\win-x64\native\comhost.dll");
            var addr2 = injector.Inject(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\6.0.7\runtimes\win-x64\native\ijwhost.dll");
            var addr3 = injector.Inject(@"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\6.0.7\runtimes\win-x64\native\nethost.dll");

            var dll = Path.Combine(AppContext.BaseDirectory, "QuakePluginsHook.dll");
            var addr4 = injector.Inject(dll);

            var parameters = new DotnetInitializeParameters();
            parameters.DllPath = new byte[256];
            Encoding.ASCII.GetBytes(dll).CopyTo(parameters.DllPath, 0);

            injector.CallFunction("QuakePluginsHook.dll", "dotnet_initialize", parameters, true);

            //DllInjector.Inject(processes[0], Path.Combine(AppContext.BaseDirectory, "QuakePluginsHook.dll"),"dotnet_initialize");

            System.Console.WriteLine("Injected! Enjoy");
        }
    }
}
