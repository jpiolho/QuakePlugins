#pragma once

#include <nethost.h>
#include <hostfxr.h>
#include "adapters.hpp"
#include <coreclr_delegates.h>

typedef void (CORECLR_DELEGATE_CALLTYPE* QuakeEnhancedServerAnnouncer_SetupInterop_fn)(Adapters* adapters);

typedef void (CORECLR_DELEGATE_CALLTYPE* QuakeEnhancedServerAnnouncer_ReceiveServerJson_fn)(char* json);
extern QuakeEnhancedServerAnnouncer_ReceiveServerJson_fn QuakeEnhancedServerAnnouncer_ReceiveServerJson;

typedef void (CORECLR_DELEGATE_CALLTYPE* QuakeEnhancedServerAnnouncer_OnServerBrowserIdle_fn)();
extern QuakeEnhancedServerAnnouncer_OnServerBrowserIdle_fn QuakeEnhancedServerAnnouncer_OnServerBrowserIdle;

typedef void (CORECLR_DELEGATE_CALLTYPE* QuakeEnhancedServerAnnouncer_MainInjected_fn)();
extern QuakeEnhancedServerAnnouncer_MainInjected_fn QuakeEnhancedServerAnnouncer_MainInjected;

typedef bool (CORECLR_DELEGATE_CALLTYPE* QuakeEnhancedServerAnnouncer_OnLobbyRender_fn)();
extern QuakeEnhancedServerAnnouncer_OnLobbyRender_fn QuakeEnhancedServerAnnouncer_OnLobbyRender;
