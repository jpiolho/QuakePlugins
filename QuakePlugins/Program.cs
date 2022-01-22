using QuakePlugins.Addons;
using QuakePlugins.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
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
            Debugger.Launch();

            try
            {
                QEngine.InitializeQEngine();

                _addonsManager = new AddonsManager();
                _addonsManager.Start();

                Quake.SetupHooks();
            }
            catch(Exception ex)
            {
                Quake.PrintConsole("Exception: " + ex.ToString() + "\n", System.Drawing.Color.Red);
            }
            
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
