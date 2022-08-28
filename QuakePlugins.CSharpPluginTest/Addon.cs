using QuakePlugins.Plugins;
using QuakePlugins.API;

namespace QuakePlugins.CSharpPluginTest
{
    public class MyAddon : Plugin
    {
        Cvar cvar;
        protected override void OnInitialize()
        {
            cvar = Cvars.Register("csharpcvar", "hello there", "Test CSharp cvar registered from C#");

            API.Console.PrintLine("\n\nPrinting from C#!\n\n");
        }
    }
}