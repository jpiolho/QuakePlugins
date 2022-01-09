#include <Windows.h>

BOOL __cdecl HookFunction(ULONG_PTR OriginalFunction, ULONG_PTR NewFunction);
void __cdecl UnhookFunction(ULONG_PTR Function);
ULONG_PTR __cdecl GetOriginalFunction(ULONG_PTR Hook);

void InitializeNtHookEngine();