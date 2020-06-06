#pragma once
#include "Header.h"
#include "worker.h"
#include "functions.h"
#include "logger.h"

class ApplyEdition;
class DestroyMessage;
class HistoryMessage;
class Malloc;
class LoggerManager;
class SetText;

BOOL hook(LPVOID pfnOld, LPVOID pfnNew, PBYTE pOrgBytes);
BOOL unhook(LPVOID pfnOld, LPVOID pfnNew, PBYTE pOrgBytes);
BOOL getTelegramFunctions();
void loop();
DWORD seekPattern(const char* pattern, SIZE_T patternSize, char* base, SIZE_T baseSize);

struct MARK_INFO
{
	std::wstring LangName;
	std::wstring Content;
	INT Width;
};

namespace g
{
#ifdef __cplusplus
	extern "C" {
#endif
	extern ApplyEdition applyEdition;
	extern DestroyMessage destroyMessage;
	extern Malloc fnMalloc;
	extern Free fnFree;
	extern SetText setText;
	extern QStringFromAscii qStringFromAscii;
	extern std::set<HistoryMessage*> RevokedMessages;
	namespace Offsets
	{
		extern ULONG TimeText;
		extern ULONG TimeWidth;
		extern ULONG MainView;
		extern ULONG Media;
	}
	extern MARK_INFO CurrentMark;
	extern LoggerManager Logger;
	extern mutex Mutex;
#ifdef __cplusplus
	}
#endif
}