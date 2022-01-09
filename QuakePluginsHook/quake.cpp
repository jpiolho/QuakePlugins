#include "quake.hpp"
#include <memory>
#include <Windows.h>
#include <xmmintrin.h>
#include <string>


void Quake_PrintConsole(const char* text, int color)
{
	typedef void(__fastcall* PrintConsoleFn)(const void* vTable, const int* color, const char* text);
	((PrintConsoleFn)0x1400d69a0)((void*)0x1409bf140, &color, text);
}

Cvar_t* Quake_RegisterCvar(const char* name, const char* description, const char* defaultValue, const int flags, const float min, const float max, const int unk1, const int unk2, const bool unk3, const void* callback)
{
	auto cvar = new Cvar_t();

	
	
	typedef void*(__fastcall* RegisterCvarFn)(const void* cvar, const char* name, const char* defaultValue, const char* description, const int flags, const float min,const float max,const bool unk3, const void* callback);
	((RegisterCvarFn)0x1400da2c0)(cvar, name, defaultValue, description, flags,min,max,unk3,callback);
	
	
	/*
	typedef void*(__fastcall* RegisterCvarFn)(const void* cvar, const char* name, const char* defaultValue, const char* description, const int flags, const float max, const float min);
	((RegisterCvarFn)0x1400d6f70)(cvar, name, defaultValue, description, flags, max, min);
	*/

	/*
	cvar->array1.unknown1 = 1;
	cvar->array2.unknown1 = 1;

	typedef void (__fastcall* SetKexArrayCapacityFn)(const void* arr, const int capacity);
	((SetKexArrayCapacityFn)0x1400d7a50)(&cvar->array1, 1);
	cvar->array1.length = 1;

	((SetKexArrayCapacityFn)0x1400d7a50)(&cvar->array2, 1);	
	cvar->array2.length = 1;

	*/

	// Re-build cvar commands
	*(void**)(0x149e0c178 + 24) = nullptr;


	return cvar;
}

float Quake_GetCvarFloatValue(const Cvar_t* cvar, const int defaultValue) {
	typedef float(*GetCvarDoubleValueFn)(const Cvar_t* cvar, const int defaultValue);
	return ((GetCvarDoubleValueFn)0x1400dac50)(cvar, defaultValue);
}

const char* __stdcall Quake_GetCvarStringValue(const Cvar_t* cvar) {
	return *(const char**)(cvar->array1.data);
}