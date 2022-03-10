using QuakePlugins.Addons;
using QuakePlugins.Engine;
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

namespace QuakePlugins
{
    public static class Program
    {
        public delegate void MainInjectedDelegate();

        public static AddonsManager _addonsManager;

        static void MainInjected()
        {
            //Debugger.Launch();

            try
            {
                QEngine.InitializeQEngine();

                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

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


        static void Main(string[] args)
        {
            var processes = Process.GetProcessesByName("Quake_x64_steam");

            if (processes.Length < 1)
                throw new Exception("Could not find process");

            if (processes.Length > 1)
                throw new Exception("Too many processes found");

            DllInjector.Inject(processes[0], Path.Combine(AppContext.BaseDirectory, "QuakePluginsHook.dll"),"dotnet_initialize");

            Console.WriteLine("Injected! Enjoy");
        }
    }
}
