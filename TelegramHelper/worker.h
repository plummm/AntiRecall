#pragma once
#include "global.h"
#include "QtString.h"
#include <string>

namespace File
{
	ULONG				GetCurrentVersion();
}

namespace Text
{
	std::string			ToLower(const std::string& String);
	std::string			SubReplace(const std::string& Source, const std::string& Target, const std::string& New);
	std::vector<std::string> SplitByFlag(const std::string& Source, const std::string& Flag);
	std::string			Format(const CHAR* Format, ...);
}

template <typename T> T CallVirtual(PVOID Base, ULONG Index)
{
	return (T)(((PVOID**)Base)[0][Index]);
}

namespace Convert
{
	std::string			UnicodeToAnsi(const std::wstring& String);
}

namespace Safe
{
	template <typename T1, typename T2> BOOLEAN Except(T1 TryCallback, T2 ExceptCallback)
	{
		__try
		{
			TryCallback();
			return TRUE;
		}
		__except (EXCEPTION_EXECUTE_HANDLER)
		{
			ExceptCallback(GetExceptionCode());
			return FALSE;
		}
	}
}

enum DocumentType {
	FileDocument = 0,
	VideoDocument = 1,
	SongDocument = 2,
	StickerDocument = 3,
	AnimatedDocument = 4,
	VoiceDocument = 5,
	RoundVideoDocument = 6,
	WallPaperDocument = 7,
};


class Object
{
public:
	INT GetWidth();
	void SetWidth(INT Value);

	PVOID VirtualTable = NULL;
	INT MaxWidth = 0;
	INT MinHeight = 0;
	INT Width = 0;
	INT Height = 0;
};

class DocumentData
{
public:
	ULONG GetType();
	BOOLEAN IsSticker();
};

class Media : public Object
{
public:
	DocumentData* GetDocument();
};

class HistoryViewElement : public Object
{
public:
	Media* GetMedia();
};

class HistoryMessageEdited
{
public:
	QtString* GetTimeText();
};


class HistoryMessage
{
public:
	BOOLEAN toHistoryMessage();
	void* GetMessageBadge();

	HistoryMessageEdited* GetEdited();
	Media* GetMedia();
	BOOLEAN IsSticker();
	BOOLEAN IsLargeEmoji();
	HistoryViewElement* GetMainView();
	QtString* GetTimeText();
	INT GetTimeWidth();
	void SetTimeWidth(INT Value);
};