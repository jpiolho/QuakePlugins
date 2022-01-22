#include <Windows.h>
#include <string>
#include <locale>
#include <codecvt>

#include "dotnet.hpp"
#include "quake.hpp"

#pragma unmanaged

hostfxr_initialize_for_runtime_config_fn init_fptr;
hostfxr_get_runtime_delegate_fn get_delegate_fptr;
hostfxr_close_fn close_fptr;
load_assembly_and_get_function_pointer_fn load_assembly_fptr;

QuakeEnhancedServerAnnouncer_MainInjected_fn QuakeEnhancedServerAnnouncer_MainInjected;


std::wstring dll_folder;


#define ConsoleColorRegular 0xFFde9414
#define ConsoleColorError 0xFF2330de



std::wstring append_path(const std::wstring left, const std::wstring right)
{
    return left + L"\\" + right;
}

bool get_function_pointers() {
    // Pre-allocate a large buffer for the path to hostfxr
    char_t buffer[MAX_PATH];
    size_t buffer_size = sizeof(buffer) / sizeof(char_t);
    int rc = get_hostfxr_path(buffer, &buffer_size, nullptr);
    if (rc != 0)
        return false;

    // Load hostfxr and get desired exports
    auto lib = LoadLibraryW(buffer);
    if (lib == nullptr)
        return false;

    init_fptr = (hostfxr_initialize_for_runtime_config_fn)GetProcAddress(lib, "hostfxr_initialize_for_runtime_config");
    get_delegate_fptr = (hostfxr_get_runtime_delegate_fn)GetProcAddress(lib, "hostfxr_get_runtime_delegate");
    close_fptr = (hostfxr_close_fn)GetProcAddress(lib, "hostfxr_close");

    return (init_fptr && get_delegate_fptr && close_fptr);
}

// Load and initialize .NET Core and get desired function pointer for scenario
load_assembly_and_get_function_pointer_fn get_dotnet_load_assembly(const std::wstring config_path)
{
    // Load .NET Core
    void* load_assembly_and_get_function_pointer = nullptr;
    hostfxr_handle cxt = nullptr;
    int rc = init_fptr(config_path.c_str(), nullptr, &cxt);
    if (rc != 0 || cxt == nullptr)
    {
        std::wstring message = L"Init failed: " + std::to_wstring(rc);
        MessageBoxW(NULL, message.c_str(), L"Initialization failed", MB_OK);

        close_fptr(cxt);
        return nullptr;
    }

    // Get the load assembly function pointer
    rc = get_delegate_fptr(
        cxt,
        hdt_load_assembly_and_get_function_pointer,
        &load_assembly_and_get_function_pointer);

    if (rc != 0 || load_assembly_and_get_function_pointer == nullptr) {
        std::wstring message = L"Get delegate failed: " + std::to_wstring(rc);
        MessageBoxW(NULL, message.c_str(), L"Initialization failed", MB_OK);

        close_fptr(cxt);
        return nullptr;
    }

    close_fptr(cxt);

    load_assembly_fptr = (load_assembly_and_get_function_pointer_fn)load_assembly_and_get_function_pointer;
    return load_assembly_fptr;
}


void* get_dotnet_function(std::wstring functionName,std::wstring functionNamespace,std::wstring functionDelegate) {
    //typedef void (CORECLR_DELEGATE_CALLTYPE* initialize_fn)();
    void* functionPtr = nullptr;
    int rc = load_assembly_fptr(
        append_path(dll_folder,L"QuakePlugins.dll").c_str(),
        functionNamespace.c_str(),
        functionName.c_str(),
        functionDelegate.c_str(), /*delegate_type_name*/
        nullptr,
        (void**)&functionPtr);

   
    return functionPtr;
}

std::wstring string_to_wstring(const std::string& str)
{
    int count = MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.length(), NULL, 0);
    std::wstring wstr(count, 0);
    MultiByteToWideChar(CP_ACP, 0, str.c_str(), str.length(), &wstr[0], count);
    return wstr;
}

std::wstring get_dll_directory(const std::wstring dll_path) {
    std::wstring::size_type pos = std::wstring(dll_path).find_last_of(L"\\/");
    return std::wstring(dll_path).substr(0, pos);
}


void PrintConsole(std::string text) {
    Quake_PrintConsole((std::string("[QuakePlugins] ") + text + "\n").c_str(), ConsoleColorRegular);
}

void PrintConsoleError(std::string text) {
    Quake_PrintConsole((std::string("[QuakePlugins] ") + text + "\n").c_str(), ConsoleColorError);
}


int dotnet_initialize(char* dllPath) {

    PrintConsole("Initializing dotnet...");

    dll_folder = get_dll_directory(string_to_wstring(std::string(dllPath)));

    if (!get_function_pointers()) {
        PrintConsoleError("Failed to get function pointers");
        return -1;
    }

    if (get_dotnet_load_assembly(append_path(dll_folder, L"QuakePlugins.runtimeconfig.json")) == nullptr) {
        PrintConsoleError("Failed to load dotnet assembly");
        return -2;
    }

    
    PrintConsole("Getting dotnet functions...");

    QuakeEnhancedServerAnnouncer_MainInjected = (QuakeEnhancedServerAnnouncer_MainInjected_fn)get_dotnet_function(L"MainInjected",
        L"QuakeEnhancedServerAnnouncer.Program, QuakePlugins",
        L"QuakeEnhancedServerAnnouncer.Program+MainInjectedDelegate, QuakePlugins"
    );
    if (QuakeEnhancedServerAnnouncer_MainInjected == nullptr) {
        PrintConsoleError("Failed to find dotnet MainInjected function");
        return -6;
    }

    PrintConsole("Calling dotnet main...");

    QuakeEnhancedServerAnnouncer_MainInjected();

    PrintConsole("Ready");


    return 0;
}