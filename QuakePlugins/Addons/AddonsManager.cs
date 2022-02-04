using QuakePlugins;
using System;
using System.Collections.Generic;
using System.IO;

namespace QuakePlugins.Addons
{
    public class AddonsManager
    {
        private List<Addon> _addons;

        public IReadOnlyList<Addon> Addons => _addons.AsReadOnly();

        internal AddonsManager()
        {
            _addons = new List<Addon>();
        }

        internal void Start()
        {
            _addons.Clear();

            foreach (var folder in new DirectoryInfo(Path.Combine("rerelease","_addons")).GetDirectories())
            {
                // Skip disabled addons
                if (folder.Name.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase))
                    continue;

                Quake.PrintConsole($"Loading addon '{folder}'...\n");
                
                var addon = new Addon(folder.FullName);
                addon.Load();

                _addons.Add(addon);
            }
        }
    }
}
