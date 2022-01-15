#include "hooks.hpp"
#include "dotnet.hpp"
#include <NtHookEngine.hpp>
#include <string>



struct PlayfabResponse {
    char* json;
};

bool HookServerBrowserResponse(void* arg1, PlayfabResponse* arg2, void* arg3) {

    QuakeEnhancedServerAnnouncer_ReceiveServerJson(arg2->json);

    typedef bool(__fastcall* OriginalFunction)(void*, void*, void*);
    return ((OriginalFunction)GetOriginalFunction((ULONG_PTR)HookServerBrowserResponse))(arg1,arg2,arg3);
}

bool HookServerBrowserIdle(void* arg4, void* arg5, void* arg6, void* arg7) {

    QuakeEnhancedServerAnnouncer_OnServerBrowserIdle();

    typedef bool(__fastcall* OriginalFunction)(void*, void*, void*, void*);
    return ((OriginalFunction)GetOriginalFunction((ULONG_PTR)HookServerBrowserIdle))(arg4, arg5, arg6, arg7);
}

void HookLobbyRender()
{
    if (QuakeEnhancedServerAnnouncer_OnLobbyRender())
        return;

    typedef void(__fastcall* OriginalFunction)();
    ((OriginalFunction)GetOriginalFunction((ULONG_PTR)HookLobbyRender))();
}


void OtherFunc(int* function) {
    void* pr_functions = (void*)*((int*)0x1418a2a28);
    char** pr_strings = *((char***)0x141a4a600);
    int nameIndex = *(int*)((long long)function + 16);
    char* name = (char*)((long long)pr_strings + nameIndex);
    auto debugText = std::string("EnterFunction: ") + std::string(name) + std::string("\n");

    adapter_PrintConsole(debugText.c_str(), 0xFF00FFFF);
}

void Hook_PR_EnterFunction(int* function) {
    
    OtherFunc(function);
   

    typedef void(__fastcall* OriginalFunction)(int* function);
    ((OriginalFunction)GetOriginalFunction((ULONG_PTR)Hook_PR_EnterFunction))(function);
}

void hooks_initialize()
{
    InitializeNtHookEngine();

    /*
    HookFunction((ULONG_PTR)0x140633730, (ULONG_PTR)HookServerBrowserResponse);
    HookFunction((ULONG_PTR)0x140354d30, (ULONG_PTR)HookServerBrowserIdle);

    HookFunction((ULONG_PTR)0x140356620, (ULONG_PTR)HookLobbyRender);
    */

    HookFunction((ULONG_PTR)0x1401c7390, (ULONG_PTR)Hook_PR_EnterFunction);
}
