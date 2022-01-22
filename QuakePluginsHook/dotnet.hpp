#pragma once

#include <nethost.h>
#include <hostfxr.h>
#include <coreclr_delegates.h>

typedef void (CORECLR_DELEGATE_CALLTYPE* QuakeEnhancedServerAnnouncer_MainInjected_fn)();
extern QuakeEnhancedServerAnnouncer_MainInjected_fn QuakeEnhancedServerAnnouncer_MainInjected;