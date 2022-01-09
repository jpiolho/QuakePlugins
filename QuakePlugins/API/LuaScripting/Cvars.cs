using QuakeEnhancedServerAnnouncer;

namespace QuakePlugins.API.LuaScripting
{
    internal class Cvars
    {
        public static void Register(string name, string defaultValue, string description = "")
        {
            Quake.RegisterCvar(name, description, defaultValue, 0x0, 0f, 1f);
        }


    }
}
