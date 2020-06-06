#pragma once
#include <Windows.h>
#include <tchar.h>
#include <Psapi.h>
#include <TlHelp32.h>
#include <algorithm>
#include <set>
#include <string>
#include <queue>
#include <mutex>

#pragma comment(lib, "wininet.lib")
#pragma comment(lib, "Version.lib")

#define VERSION        "0.2.0"
#define URL_ISSUES     "https://github.com/plummm/AntiRecall/issues"

#define TO_STRING(v)							# v
#define PAGE_SIZE								( 0x1000 )
#define FORCE_EXIT()							{ TerminateProcess((HANDLE)-1, 0); ExitProcess(0); }

typedef INT(__cdecl* PFAPPLYEDITION)();
typedef BOOL(*PFGETTELEGRAMFUNCTIONS)();
typedef LPVOID(__cdecl* PFMALLOC)(unsigned int size);
typedef void(__cdecl* PFFREE)(void* block);
typedef void(__cdecl* PFSETTEXT)(void*, void*, void*);
typedef void*(__cdecl* PFQSTRINGFROMASCII)(const char*, int);

using namespace std;

namespace Text
{
	std::string			ToLower(const std::string& String);
	std::string			SubReplace(const std::string& Source, const std::string& Target, const std::string& New);
	std::vector<std::string> SplitByFlag(const std::string& Source, const std::string& Flag);
	std::string			Format(const CHAR* Format, ...);
	class String 
	{
	public:
		void* GetText() 
		{
			return (void*)*(DWORD*)(this + 0xc);
		}
	};
}