using QuakeAddons.Addons;
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

namespace QuakeEnhancedServerAnnouncer
{
    public static class Program
    {
        private static Quake.Cvar cvar_interval, cvar_interval_margin;
        private static Quake.Cvar cvar_webhook;
        private static Quake.Cvar cvar_enabled;
        private static Quake.Cvar cvar_announce_close;
        private static Quake.Cvar cvar_debug;
        private static Quake.Cvar cvar_whitelist;

        private static Quake.Cvar cvar_lobbycycler_enabled;
        private static Quake.Cvar cvar_lobbycycler_list;

        public delegate void ReceiveServerJsonDelegate(IntPtr rawJsonPointer);
        public delegate void OnServerBrowserIdleDelegate();
        public delegate void MainInjectedDelegate();
        public delegate bool OnLobbyRenderDelegate();

        private static JsonSerializerOptions JsonOptions = new JsonSerializerOptions()
        {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        /*
        static async Task RunServerAsync(CancellationToken cancellationToken)
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:15233/");
            listener.Start();


            cancellationToken.Register(() => listener.Stop());


            while (true)
            {
                var context = await listener.GetContextAsync();
                if (context.Request.HttpMethod != "POST")
                {
                    context.Response.StatusCode = (int)HttpStatusCode.MethodNotAllowed;
                    context.Response.Close();
                    continue;
                }

                using (var reader = new StreamReader(context.Request.InputStream))
                {
                    var result = await reader.ReadToEndAsync();
                    var query = HttpUtility.ParseQueryString(result);

                    var json = JsonSerializer.Deserialize<FunctionExecutionResult>(query["json"], JsonOptions);

                    await HandleJsonAsync(json, cancellationToken);

                    context.Response.StatusCode = (int)HttpStatusCode.OK;
                    context.Response.Close();
                }
            }
        }
        */

        static async Task SendWebhookMessageAsync(string title,string message, int color,string webhook, CancellationToken cancellationToken)
        {
            using var http = new HttpClient();

            var data = new
            {
                embeds = new[]
                {
                    new {
                        title = title,
                        description = message,
                        color = color
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(data, JsonOptions), Encoding.UTF8, "application/json");

            //  "https://discord.com/api/webhooks/908052204357832774/9h1qrud9tDAin91iONvqUSV_V-CUtfU0T3HR4a0mMVhtWpxzvIdvhv9W02eos8LOMLyB";
            await http.PostAsync(webhook, content, cancellationToken);
        }

        private static string ModeToString(string mode)
        {
            switch(mode)
            {
                case "$m_deathmatch": return "Deathmatch";
                case "$m_teamplay": return "Teamplay";
                case "$m_coop": return "Cooperative";
                default: return "Unknown";
            }
        }

        private static string GameToString(string game)
        {
            switch(game.ToUpperInvariant())
            {
                default: return "Unknown";
                case "ID1": return "Quake";
                case "MG1": return "Dimension of the Machine";
                case "DOPA": return "Dimension of the Past";
                case "ROGUE": return "Dissolution of Eternity";
                case "HIPNOTIC": return "Scourge of Armagon";
            }
        }

        private static Dictionary<string, LobbyValue> lobbies;
        static async Task HandleJsonAsync(FunctionExecutionResult result, CancellationToken cancellationToken)
        {
            if (result.Code != 200)
                return;

            var firstTime = lobbies == null;

            if (firstTime) lobbies = new Dictionary<string, LobbyValue>();

            var whitelist = cvar_whitelist.GetStringValue().Trim().ToUpperInvariant().Split(",");
            var webhook = cvar_webhook.GetStringValue();
            var debug = cvar_debug.GetFloatValue(0) == 1;

            foreach (var lobby in result.Data.FunctionResult.LobbyArray)
            {
                var ingame = lobby.Value.Ingame == "1";
                if (!ingame)
                    continue;

                if (!firstTime && !lobbies.ContainsKey(lobby.Key))
                {
                    // Filter whitelist
                    if (whitelist.Length > 0 && !whitelist.Contains(lobby.Value.Name.ToUpperInvariant()))
                    {
                        if (debug)
                            Quake.PrintConsole($"[ServerAnnouncer] Skipping '{lobby.Value.Name}' for not being in the whitelist");

                        continue;
                    }

                    Quake.PrintConsole($"[ServerAnnouncer] '{lobby.Value.Name}' started a lobby\n");

                    await SendWebhookMessageAsync("New lobby",$"'{lobby.Value.Name}' started a «{GameToString(lobby.Value.Game)}» {ModeToString(lobby.Value.Mode)} [{lobby.Value.Map}] ({lobby.Value.kMaxPlayers}P) lobby", 4062976, webhook, cancellationToken);

                }

                lobbies[lobby.Key] = lobby.Value;
            }

            foreach(var lobby in lobbies.Reverse())
            {
                if(!result.Data.FunctionResult.LobbyArray.Any(l => l.Key == lobby.Key))
                {
                    Quake.PrintConsole($"[ServerAnnouncer] '{lobby.Value.Name}' lobby ended\n");

                    if(cvar_announce_close.GetFloatValue(0) == 1)
                        await SendWebhookMessageAsync("Ended lobby",$"'{lobby.Value.Name}' ended the lobby", 16711680, webhook, cancellationToken);


                    lobbies.Remove(lobby.Key);
                }
            }

            var interval = cvar_interval.GetFloatValue(60);
            var margin = cvar_interval_margin.GetFloatValue(10);
            margin = Math.Min(margin, interval - 30);

            var intervalMin = interval - margin;
            var intervalMax = interval + margin;
            nextRefresh = DateTime.Now.AddSeconds(RandomFloat(intervalMin,intervalMax+1));
        }

        private static float RandomFloat(float min,float max)
        {
            return (float)(rnd.NextDouble() * (max - min) + min);
        }

        private static Random rnd = new Random();
        private static DateTime nextRefresh;
        static unsafe void OnServerBrowserIdle()
        {
            if (cvar_enabled.GetFloatValue(0) != 1)
                return;

            if (DateTime.Now > nextRefresh)
            {
                *(int*)0x149dd8afc = 0x800; // Simulate pressing the refresh key

                Quake.PrintConsole("[ServerAnnouncer] Refreshing list...\n");

                nextRefresh = DateTime.Now.AddSeconds(5); // Fail safe in case refreshing fails
            }
        }



        
        private static bool inLobby = false;
        private static DateTime lobbyStartTime;
        private static IntPtr lobbyGlobalMod, lobbyGlobalMap;
        static unsafe bool OnLobbyRender()
        {
            if (cvar_lobbycycler_enabled.GetFloatValue(0) != 1)
                return false;

            if(!inLobby)
            {
                Quake.PrintConsole("[LobbyCycler] Starting in 2 seconds...\n", 16711680);
                lobbyStartTime = DateTime.Now.AddSeconds(2);
                inLobby = true;
            }

            if(inLobby & DateTime.Now >= lobbyStartTime)
            {
                if ((DateTime.Now - lobbyStartTime) > TimeSpan.FromSeconds(10))
                {
                    inLobby = false;
                    return false;
                }

                if(lobbyGlobalMod == IntPtr.Zero)
                {
                    lobbyGlobalMod = Marshal.AllocHGlobal(128);
                    lobbyGlobalMap = Marshal.AllocHGlobal(128);
                }

                var currentMod = Marshal.PtrToStringAnsi(new IntPtr(*(char**)(0x149d3d370 + 0x10)));
                var currentMap = Marshal.PtrToStringAnsi(new IntPtr(*(char**)(0x149d3d370 + 0x28)));

                Quake.PrintConsole($"[LobbyCycler] Current mod: {currentMod} | Current map: {currentMap}\n");


                var lobbyCyclerList = cvar_lobbycycler_list.GetStringValue();

                var mapsInCycler = new List<(string, string)>();

                foreach(var entry in lobbyCyclerList.Trim().Split(";"))
                {
                    var split = entry.Trim().Split(",");

                    if (split.Length != 2)
                        continue;

                    mapsInCycler.Add((split[0], split[1]));
                }

                // No maps in cycler, ignore
                if (mapsInCycler.Count == 0)
                {
                    Quake.PrintConsole($"[LobbyCycler] Attempted to start game, but no cycler list specified.\n", 16711680);
                    return false;
                }

                (string, string) nextMapMod = mapsInCycler[0];
                for(var i=0;i<mapsInCycler.Count;i++)
                {
                    var entry = mapsInCycler[i];
                    if (entry.Item1.Equals(currentMod, StringComparison.OrdinalIgnoreCase) && entry.Item2.Equals(currentMap, StringComparison.OrdinalIgnoreCase))
                    {
                        if (++i == mapsInCycler.Count)
                            i = 0;

                        nextMapMod = mapsInCycler[i];
                        break;
                    }
                }

                Quake.ChangeGame(nextMapMod.Item1);

                var buffer = Encoding.ASCII.GetBytes($"{nextMapMod.Item1}\0");
                Marshal.Copy(buffer, 0, lobbyGlobalMod, buffer.Length);
                buffer = Encoding.ASCII.GetBytes($"{nextMapMod.Item2}\0");
                Marshal.Copy(buffer, 0, lobbyGlobalMap, buffer.Length);

                *(int*)(0x149d3d370 + 0x10) = lobbyGlobalMod.ToInt32();
                *(int*)(0x149d3d370 + 0x28) = lobbyGlobalMap.ToInt32();

                Quake.PrintConsole($"[LobbyCycler] Starting game. Mod: {nextMapMod.Item1}, Map: {nextMapMod.Item2}\n", 16711680);

                Quake.StartServerGame();

                inLobby = false;

                return true;
            }

            return false;
        }


        static async void ReceiveServerJson(IntPtr rawJsonPointer)
        {
            if (cvar_enabled.GetFloatValue(0) != 1)
                return;

            var rawJson = Marshal.PtrToStringAnsi(rawJsonPointer);

            if(cvar_debug.GetFloatValue(0) == 1)
                Quake.PrintConsole($"[ServerAnnouncer] JSON: " + rawJson + "\n",0xFF00FF00);

            var cts = new CancellationTokenSource();

            try
            {
                var json = JsonSerializer.Deserialize<FunctionExecutionResult>(rawJson, JsonOptions);

                await HandleJsonAsync(json, cts.Token);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Exception: " + ex.ToString());
            }
        }


        static AddonsManager _addonsManager;

        static void MainInjected()
        {
            cvar_interval = Quake.RegisterCvar("serverannouncer_interval", "Interval of time in seconds between refreshing the server list", "60", 0x14, 30, 3600);
            cvar_interval_margin = Quake.RegisterCvar("serverannouncer_interval_margin", "A margin of time in seconds that gets randomly applied to the interval", "10", 0x14, 0, 3600);
            cvar_webhook = Quake.RegisterCvar("serverannouncer_discord_webhook", "Discord webhook url to which to send the alerts", "", 0x0, 0, 0);
            cvar_enabled = Quake.RegisterCvar("serverannouncer_enabled", "Enables or disables the server announcer", "0", 0x1, 0, 1);
            cvar_announce_close = Quake.RegisterCvar("serverannouncer_announce_close", "If enabled, sends a message when a lobby gets closed", "1", 0x1, 0, 1);
            cvar_debug = Quake.RegisterCvar("serverannouncer_debug", "If enabled, prints debugging messages", "0", 0x1, 0, 1);
            cvar_whitelist = Quake.RegisterCvar("serverannouncer_whitelist", "List of allowed user names to generate the alerts for", "", 0x0, 0, 1);

            cvar_lobbycycler_enabled = Quake.RegisterCvar("lobbycycler_enabled", "Enables or disables the lobby cycler", "0", 0x1, 0, 1);
            cvar_lobbycycler_list = Quake.RegisterCvar("lobbycycler_list", "List of mods/maps the lobby should cycle. Format: mod,map;mod,map;...", "", 0x0, 0, 1);

            try
            {
                _addonsManager = new AddonsManager();
                _addonsManager.Start();
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

            DllInjector.Inject(processes[0], Path.Combine(AppContext.BaseDirectory, "QuakeAddonsHook.dll"),"dotnet_initialize");

            Console.WriteLine("Injected! Enjoy");
        }
    }
}
