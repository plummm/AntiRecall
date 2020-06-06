#include "global.h"

DWORD seekPattern(const char* pattern, SIZE_T patternSize, char* base, SIZE_T baseSize)
{
	int index = 0;
	for (UINT32 i = 0; i < baseSize; i++) {
		if (base[i] == pattern[index]) {
			index++;
		}
		else {
			index = 0;
		}
		if (index == patternSize)
			return i - patternSize + 1;
	}
	return -1;
}

BOOL getTelegramFunctions()
{
	HMODULE hMod;
	PBYTE pAddr;
	int index;
	MODULEINFO pModInfo = { 0, };

	hMod = GetModuleHandle(NULL);
	pAddr = (PBYTE)hMod;

	GetModuleInformation(GetCurrentProcess(), hMod, &pModInfo, sizeof(pModInfo));

	//malloc
	index = seekPattern(g::fnMalloc.pattern, g::fnMalloc.patternSize, (char*)pModInfo.lpBaseOfDll, pModInfo.SizeOfImage);
	if (index == -1) {
		return false;
	}
	g::fnMalloc.fn = (PFMALLOC)g::fnMalloc.GetFunc((DWORDLONG)pModInfo.lpBaseOfDll + index);

	//free
	index = seekPattern(g::fnFree.pattern, g::fnFree.patternSize, (char*)pModInfo.lpBaseOfDll, pModInfo.SizeOfImage);
	if (index == -1) {
		return false;
	}
	g::fnFree.fn = (PFFREE)g::fnFree.GetFunc((DWORDLONG)pModInfo.lpBaseOfDll + index);
	g::fnFree.addr = g::fnFree.GetFuncAddress((DWORDLONG)pModInfo.lpBaseOfDll + index);

	//applyEdition
	index = seekPattern(g::applyEdition.pattern, g::applyEdition.patternSize, (char*)pModInfo.lpBaseOfDll, pModInfo.SizeOfImage);
	if (index == -1) {
		return false;
	}
	g::applyEdition.fn = (PFAPPLYEDITION)g::applyEdition.GetFunc((DWORDLONG)pModInfo.lpBaseOfDll + index);

	//destroyMessage
	index = seekPattern(g::destroyMessage.pattern, g::destroyMessage.patternSize, (char*)pModInfo.lpBaseOfDll, pModInfo.SizeOfImage);
	if (index == -1) {
		return false;
	}
	g::destroyMessage.addr = g::destroyMessage.GetFuncAddress((DWORDLONG)pModInfo.lpBaseOfDll + index);
	/*
	//UI::Text::String::setText
	index = seekPattern(g::setText.pattern, g::setText.patternSize, (char*)pModInfo.lpBaseOfDll, pModInfo.SizeOfImage);
	if (index == -1) {
		return false;
	}
	g::setText.fn = (PFSETTEXT)g::setText.GetFunc((DWORDLONG)pModInfo.lpBaseOfDll + index);

	//QStringFromAscii
	index = seekPattern(g::qStringFromAscii.pattern, g::qStringFromAscii.patternSize, (char*)pModInfo.lpBaseOfDll, pModInfo.SizeOfImage);
	if (index == -1) {
		return false;
	}
	g::qStringFromAscii.fn = (PFQSTRINGFROMASCII)g::qStringFromAscii.GetFunc((DWORDLONG)pModInfo.lpBaseOfDll + index);
	*/
	return TRUE;
}
