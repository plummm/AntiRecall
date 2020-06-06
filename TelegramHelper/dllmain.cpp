// dllmain.cpp : Defines the entry point for the DLL application.
#include "pch.h"
#include "global.h"
#include "myFunc.h"

//#define DEBUG _DEBUG

#if _DEBUG
#pragma message("Compiling Debug version")
#else
#pragma message("Compiling Release version")
#endif

BYTE g_pOrgBytes[5] = { 0, };

void InitWorker();
void InitOffsets();

namespace g {
    ApplyEdition applyEdition;
    DestroyMessage destroyMessage;
    Malloc fnMalloc;
    Free fnFree;
    SetText setText;
    QStringFromAscii qStringFromAscii;
    std::set<HistoryMessage*> RevokedMessages;
    MARK_INFO CurrentMark = { L"English", L"deleted ", 12 * 6 };
    LoggerManager Logger;
    mutex Mutex;
    ULONG CurrentVersion = File::GetCurrentVersion();
    namespace Offsets
    {
        ULONG TimeText;
        ULONG TimeWidth;
        ULONG MainView;
        ULONG Media;
    }
}

BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
                     )
{
    switch (ul_reason_for_call)
    {
    case DLL_PROCESS_ATTACH:
        InitOffsets();
        InitWorker();
        break;
    case DLL_THREAD_ATTACH:
    case DLL_THREAD_DETACH:
    case DLL_PROCESS_DETACH:
        break;
    }
    return TRUE;
}

void InitWorker()
{
    if (!getTelegramFunctions()) {
        return;
    }

    hook(g::destroyMessage.addr, HandleDeleteMessage, g_pOrgBytes);
    hook(g::fnFree.addr, HandleFree, g_pOrgBytes);
}

void InitOffsets()
{
    // ver <= 2.1.7
    if (g::CurrentVersion <= 2001007) {
        g::Offsets::TimeText = 0x98;
        g::Offsets::TimeWidth = 0x9C;
        g::Offsets::MainView = 0x5C;
        g::Offsets::Media = 0x54;
    }
    // ver > 2.1.7
    else if (g::CurrentVersion > 2001007) {
        g::Offsets::TimeText = 0x88;
        g::Offsets::TimeWidth = 0x8C;
        g::Offsets::MainView = 0x54;
        g::Offsets::Media = 0x4C;
    }
}