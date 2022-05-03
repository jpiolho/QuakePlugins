#include "quake.hpp"
#include <memory>
#include <Windows.h>
#include <xmmintrin.h>
#include <string>


void Quake_PrintConsole(const char* text, int color)
{
	typedef void(__fastcall* PrintConsoleFn)(const void* vTable, const int* color, const char* text);
	((PrintConsoleFn)0x1400d84c0)((void*)0x1409cc140, &color, text);
}