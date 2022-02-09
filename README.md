# QuakePlugins
An engine mod for Quake Enhanced that adds plugin/scripting capabilities via C# and Lua.

**This is a work in progress and is not ready for general use. The code is extremely messy at the moment since right now it's trying to be a proof of concept**

Plugins repo: [QuakePlugins.Plugins](https://github.com/jpiolho/QuakePlugins.Plugins)

## How does this work?

* When running QuakePlugins.exe, it will find the quake process and inject a C++ dll.
* The C++ dll then will load .NET host allowing it to load C# dll.
* C# dll then creates hooks and interops with the engine (using [Reloaded.Hooks](https://github.com/Reloaded-Project/Reloaded.Hooks))
* External C# dlls (using native .NET) and Lua scripts are then loaded (using [NLua](https://github.com/NLua/NLua))

## Requirements 
* .NET 6.0

## Installing alpha build
1. Download the build from the Github releases.
2. Extract into a folder somewhere on your computer
3. Download plugins or write your own. Make sure you place them in the right place (`<Quake folder>/rerelease/_addons/<plugin name>/main.lua`)
4. Start game
5. Run QuakePlugins.exe, it will now inject itself into the game and load the plugins + runtime. Check console and you should be able to see a bunch of output.
6. Done!

## Lua plugins
You can find another repo with examples and misc plugins here: [QuakePlugins.Plugins](https://github.com/jpiolho/QuakePlugins.Plugins)

Plugins should be installed in the following folder: `<Quake folder>/rerelease/_addons/<plugin name>/main.lua`

Example plugin:
```lua
-- Register a new cvar
local cvar_lua_grenadeexplode = Cvars.Register("lua_grenadeexplode","1.0","Some testing cvar from lua");

function QC_GrenadeExplode()

    -- Only print to console if the cvar is set to 1
    if cvar_lua_grenadeexplode:GetBool() then
        Console.Print("[Lua] A grenade exploded!\n");
    end
end

-- Hook into GrenadeExplode QC function
Hooks.RegisterQC("GrenadeExplode",QC_GrenadeExplode);

-- A 'hello world' based on your name
Console.Print("[Cvar] Hello, " .. Cvars.Get("cl_name"):GetString() .. "\n");
```
