#include "global.h"

BOOL hook(LPVOID pfnOld, LPVOID pfnNew, PBYTE pOrgBytes) {
	PBYTE pByte;
	DWORDLONG dwAddress;
	DWORD dwOldProtect;
	BYTE pBuf[5] = { 0xE8,0, };
	BYTE nopPattern[5] = { 0xc3, 0xcc, 0xcc, 0xcc, 0xcc };

	pByte = (PBYTE)pfnOld;

	if (pByte[0] == 0xE9)
		return FALSE;

	VirtualProtect(pfnOld, 5, PAGE_EXECUTE_READWRITE, &dwOldProtect);
	memcpy(pOrgBytes, pfnOld, 5);
	dwAddress = (DWORDLONG)pfnNew - (DWORDLONG)pfnOld - 5;
	memcpy(&pBuf[1], &dwAddress, 4);
	memcpy(pfnOld, pBuf, 5);
	VirtualProtect(pfnOld, 5, dwOldProtect, &dwOldProtect);

	return TRUE;
}

BOOL unhook(LPVOID pfnOld, LPVOID pfnNew, PBYTE pOrgBytes)
{
	DWORD dwOldProtect;
	PBYTE pByte;

	pByte = (PBYTE)pfnOld;

	if (pByte[0] != 0xE9)
		return FALSE;

	VirtualProtect(pfnOld, 5, PAGE_EXECUTE_READWRITE, &dwOldProtect);
	memcpy(pfnOld, pOrgBytes, 5);
	VirtualProtect(pfnOld, 5, dwOldProtect, &dwOldProtect);

	return TRUE;
}