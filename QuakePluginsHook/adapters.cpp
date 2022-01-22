#include "quake.hpp"
#include <string>

void __stdcall adapter_StartServerGame() {
	typedef float(*StartServerGameFn)(const void* pointer, const bool unk1);
	((StartServerGameFn)0x140182360)((void*)0x149db5450, true);

	return;
}

void __stdcall adapter_ChangeGame(const char* game) {
	typedef float(*ChangeGameFn)(const char* game);
	((ChangeGameFn)0x14017afe0)(game);
}