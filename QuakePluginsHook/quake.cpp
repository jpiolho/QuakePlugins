#include "quake.hpp"
#include <memory>
#include <Windows.h>
#include <xmmintrin.h>
#include <string>

void* printConsole;

void Quake_PrintConsole(const char* text, int color)
{
	typedef void(__fastcall* PrintConsoleFn)(const void* vTable, const int* color, const char* text);
	((PrintConsoleFn)printConsole)((void*)0x1409cc140, &color, text);
}