#include "adapters.hpp"
#include "quake.hpp"
#include <string>

void __stdcall adapter_PrintConsole(const char* text, int color)
{
	Quake_PrintConsole(text, color);
}


void* __stdcall adapter_RegisterCvar(const char* name, const char* description, const char* defaultValue, const int flags, const float min, const float max)
{
	char* new_name = new char[strlen(name) + 1];
	strncpy(new_name, name, strlen(name));
	new_name[strlen(name)] = '\0';

	char* new_description = new char[strlen(description) + 1];
	strncpy(new_description, description, strlen(description));
	new_description[strlen(description)] = '\0';
	
	char* new_defaultValue = new char[strlen(defaultValue) + 1];
	strncpy(new_defaultValue, defaultValue, strlen(defaultValue));
	new_defaultValue[strlen(defaultValue)] = '\0';

	return Quake_RegisterCvar(new_name, new_description, new_defaultValue, flags, min, max, 0, 0, true, nullptr);
}

float __stdcall adapter_GetCvarFloatValue(const Cvar_t* cvar, int defaultValue) {
	return Quake_GetCvarFloatValue(cvar, defaultValue);
}

const char* __stdcall adapter_GetCvarStringValue(const Cvar_t* cvar) {
	return Quake_GetCvarStringValue(cvar);
}

void __stdcall adapter_StartServerGame() {
	typedef float(*StartServerGameFn)(const void* pointer, const bool unk1);
	((StartServerGameFn)0x140182360)((void*)0x149db5450, true);

	return;
}

void __stdcall adapter_ChangeGame(const char* game) {
	typedef float(*ChangeGameFn)(const char* game);
	((ChangeGameFn)0x14017afe0)(game);
}

Adapters* Adapters_GetAdapters()
{
	auto adapters = new Adapters();

	adapters->PrintConsole = adapter_PrintConsole;
	adapters->RegisterCvar = adapter_RegisterCvar;
	adapters->GetCvarDoubleValue = adapter_GetCvarFloatValue;
	adapters->GetCvarStringValue = adapter_GetCvarStringValue;
	adapters->StartServerGame = adapter_StartServerGame;
	adapters->ChangeGame = adapter_ChangeGame;

	return adapters;
}
