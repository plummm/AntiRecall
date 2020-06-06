#pragma once
#include "Header.h"

struct QtArrayData
{
	INT ref;
	INT size;
	UINT alloc : 31;
	UINT capacityReserved : 1;
	INT offset; // in bytes from beginning of header
};

class QtString
{
public:
	QtString();
	QtString(const WCHAR* String);

	BOOLEAN IsValidTime();
	WCHAR* GetText();
	BOOLEAN IsEmpty();
	SIZE_T Find(std::wstring String);
	INT GetRefCount();
	void MakeString(const WCHAR* String);
	void Swap(QtString* Dst);
	void Replace(const WCHAR* NewContent);
	void Clear();

private:
	QtArrayData* d = NULL;

};