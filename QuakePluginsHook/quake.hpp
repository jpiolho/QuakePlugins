#pragma once

struct KexArray_t {
	void* data;
	int capacity;
	int length;
	int unknown1;
	int unknown2;
};

struct Cvar_t {
	char* name;
	char* description;
	char* defaultValue;
	int flags;
	
	char _padding[4 + 4 + 4 + 8];
	
	Cvar_t* previous;
	Cvar_t* next;
	
	char _padding2[8];

	void* callback;
	void* getString;

	char _padding3[8];

	KexArray_t array1;
	KexArray_t array2;
};

void Quake_PrintConsole(const char* text, int color);
Cvar_t* Quake_RegisterCvar(const char* name, const char* description, const char* defaultValue, const int flags, const float min, const float max, const int unk1, const int unk2, const bool unk3, const void* callback);
float Quake_GetCvarFloatValue(const Cvar_t* cvar, const int defaultValue);
const char* Quake_GetCvarStringValue(const Cvar_t* cvar);