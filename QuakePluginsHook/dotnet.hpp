#pragma once

#include <nethost.h>
#include <hostfxr.h>
#include <coreclr_delegates.h>

typedef void (CORECLR_DELEGATE_CALLTYPE* QuakePlugins_MainInjected_fn)();
extern QuakePlugins_MainInjected_fn QuakePlugins_MainInjected;