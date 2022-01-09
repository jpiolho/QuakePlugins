#include "hooks.hpp"
#include "dotnet.hpp"
#include <NtHookEngine.hpp>




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

void hooks_initialize()
{
    InitializeNtHookEngine();
    HookFunction((ULONG_PTR)0x140633730, (ULONG_PTR)HookServerBrowserResponse);
    HookFunction((ULONG_PTR)0x140354d30, (ULONG_PTR)HookServerBrowserIdle);

    HookFunction((ULONG_PTR)0x140356620, (ULONG_PTR)HookLobbyRender);
}
