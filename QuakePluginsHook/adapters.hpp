#pragma once


struct Adapters {
	void* PrintConsole;
	void* RegisterCvar;
	void* GetCvarDoubleValue;
	void* GetCvarStringValue;
	void* StartServerGame;
	void* ChangeGame;
};

void __stdcall adapter_PrintConsole(const char* text, int color);

Adapters* Adapters_GetAdapters();