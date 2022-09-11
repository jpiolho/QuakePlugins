namespace QuakePlugins.Plugins
{
    public class PluginInfo
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Author { get; set; }
        public string Version { get; set; }

        public string Type { get; set; }
        public bool? Enabled { get; set; } = false;
        public string Runtime { get; set; }
        public string Main { get; set; }
    }
}
