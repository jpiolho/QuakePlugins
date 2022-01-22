# QuakePlugins
An engine mod for Quake Enhanced that adds plugin capabilities via C# and Lua.

**This is a work in progress and is not ready for general use**

## How does this work?

* When running QuakePlugins.exe, it will find the quake process and inject a C++ dll.
* The C++ dll then will load .NET Core host allowing it to load C# dll.
* C# dll then creates hooks and interops with the engine (using [Reloaded.Hooks](https://github.com/Reloaded-Project/Reloaded.Hooks))
* External C# dlls (using native .NET) and Lua scripts are then loaded (using [NLua](https://github.com/NLua/NLua))

