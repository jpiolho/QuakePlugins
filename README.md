# QuakePlugins
An engine mod for Quake Enhanced that adds plugin capabilities via C# and Lua.

**This is a work in progress and is not ready for general use**

## How does this work?

* When running QuakePlugins.exe, it will find the quake process and inject a C++ dll.
* The C++ dll then will load .NET host allowing it to load C# dll.
* C# dll then creates hooks and interops with the engine (using [Reloaded.Hooks](https://github.com/Reloaded-Project/Reloaded.Hooks))
* External C# dlls (using native .NET) and Lua scripts are then loaded (using [NLua](https://github.com/NLua/NLua))

## Requirements 
* .NET 6.0
* comhost.dll, ijwhost.dll and nethost.dll from .NET 6.0 (`C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\6.0.1\runtimes\win-x64\native`)
* lua54.dll

## Lua plugins
Place them in `<Quake folder>/rerelease/_addons/<plugin name>/main.lua`

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
