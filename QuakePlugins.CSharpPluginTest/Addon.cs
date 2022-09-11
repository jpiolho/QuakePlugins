using QuakePlugins.Plugins;
using QuakePlugins.API;
using static QuakePlugins.API.LuaScripting.Hooks;

namespace QuakePlugins.CSharpPluginTest
{
    public class MyAddon : Plugin
    {
        Cvar cvar;
        protected override void OnInitialize()
        {
            /*
            cvar = Cvars.Register("csharpcvar", "hello there", "Test CSharp cvar registered from C#");

            API.QConsole.PrintLine("\n\nPrinting from C#!\n\n");
            */

            Builtins.RegisterCustomBuiltin("ex_cvar_string", () =>
            {
                var cvar = QC.GetString(QC.Value.Parameter0);
                var value = Cvars.Get(cvar).GetString();
                QC.SetStringTemporary(QC.Value.Return, value);
            });

            Builtins.RegisterCustomBuiltin("ex_playfab_getUniqueId", () =>
            {
                var edict = QC.GetEdict(QC.Value.Parameter0);
                QC.SetStringTemporary(QC.Value.Return, Server.GetClientByEdict(edict).GetPlayfabClient()?.UniqueId);
            });

            var coop = int.Parse(Cvars.Get("coop").GetString());

            if(coop >= 1)
            {
                Hooks.RegisterQC("PutClientInServer",PutClientInServer);
                Hooks.RegisterQC("walkmonster_start_go", SetTeamMonster);

            }
        }

        private Handling SetTeamMonster(object[] args)
        {
            QC.Self.SetField("team", 0f);

            return Handling.Handled;
        }

        private Handling PutClientInServer(object[] args)
        {
            var client = Server.GetClientByEdict(QC.Self);
            var pf = client.GetPlayfabClient();

            if(pf != null)
            {
                QConsole.PrintLine($"Playfab client: {pf.Name} | {pf.UniqueId} | {pf.NetworkId}");
            }


            QC.Self.SetField("team", 1f);
            QConsole.PrintLine($"Set coop team for {QC.Self.GetFieldString("netname")}");

            return Handling.Handled;
        }
    }
}