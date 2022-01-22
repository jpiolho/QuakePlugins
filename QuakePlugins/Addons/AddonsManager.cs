using QuakeEnhancedServerAnnouncer;
using System.Collections.Generic;
using System.IO;

namespace QuakePlugins.Addons
{
    public class AddonsManager
    {
        private List<Addon> _addons;

        public IReadOnlyList<Addon> Addons => _addons.AsReadOnly();

        internal void Start()
        {
            _addons = new List<Addon>();

            foreach (var folder in new DirectoryInfo(Path.Combine("rerelease","_addons")).GetDirectories())
            {
                Quake.PrintConsole($"Loading addon '{folder}'...\n");
                
                var addon = new Addon(folder.FullName);
                addon.Load();

                _addons.Add(addon);
            }
        }
    }
}
