using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using System.Threading.Tasks;

namespace QuakePlugins.Addons
{
    internal class AddonAssemblyLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver[] _resolvers;

        public AddonAssemblyLoadContext(string pluginPath)
        {
            _resolvers = new AssemblyDependencyResolver[]
            {
                new AssemblyDependencyResolver(pluginPath),
                new AssemblyDependencyResolver(Assembly.GetExecutingAssembly().Location)
            };
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name == Assembly.GetExecutingAssembly().GetName().Name)
                return Assembly.GetExecutingAssembly();

            foreach (var resolver in _resolvers)
            {
                string assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);
                if (assemblyPath != null)
                    return LoadFromAssemblyPath(assemblyPath);
            }


            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            foreach (var resolver in _resolvers)
            {
                string libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
                if (libraryPath != null)
                    return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
