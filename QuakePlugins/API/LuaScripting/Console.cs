using QuakeEnhancedServerAnnouncer;

namespace QuakePlugins.API.LuaScripting
{
    internal class Console
    {
        public static void Print(string text, uint? color = null)
        {
            if (color.HasValue)
                Quake.PrintConsole(text, color.Value);
            else
                Quake.PrintConsole(text);
        }

        public static void PrintLine(string text, uint? color = null)
        {
            Print(text + "\n", color);
        }
    }
}
