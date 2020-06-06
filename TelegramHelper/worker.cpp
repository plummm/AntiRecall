#include "worker.h"

//////////////////////////////////////////////////

INT Object::GetWidth()
{
	return this->MaxWidth;
}
void Object::SetWidth(INT Value)
{
	this->MaxWidth = Value;
}

//////////////////////////////////////////////////

ULONG DocumentData::GetType()
{
	return *(INT*)((ULONG_PTR)this + 8);
}

BOOLEAN DocumentData::IsSticker()
{
	return GetType() == StickerDocument;
}

//////////////////////////////////////////////////

DocumentData* Media::GetDocument()
{
	// DocumentData *Media::document()
	return *(DocumentData**)((ULONG_PTR)this + 8);
}

//////////////////////////////////////////////////

Media* HistoryViewElement::GetMedia()
{
	return *(Media**)((ULONG_PTR)this + 0x24);
}

//////////////////////////////////////////////////

QtString* HistoryMessageEdited::GetTimeText()
{
	return (QtString*)((ULONG_PTR)this + 0x10);
}

//////////////////////////////////////////////////

BOOLEAN HistoryMessage::toHistoryMessage()
{
	// HistoryMessage *HistoryItem::toHistoryMessage()
	//
	// Join channel msg is HistoryItem, that's not an inheritance class.
	// It will cause a memory access crash, so we need to filter it out.

	//return this;
	typedef HistoryMessage* (*fntToHistoryMessage)(HistoryMessage* This, ULONG a);
	return CallVirtual<fntToHistoryMessage>(this, 33)(this, 1) != NULL;
}

void* HistoryMessage::GetMessageBadge() 
{
	return (void *)(this + 0x68);
}

HistoryMessageEdited* HistoryMessage::GetEdited()
{
	HistoryMessageEdited* Result = NULL;

	Safe::Except([&]()
		{
			PVOID* pData = *(PVOID**)((ULONG_PTR)this + 8);
			INT Offset = *(INT*)((ULONG_PTR)(*pData) + 4 * g::applyEdition.fn() + 8);
			if (Offset < 4) {
				Result = NULL;
			}
			else {
				Result = (HistoryMessageEdited*)((ULONG_PTR)pData + Offset);
			}

		}, [&](ULONG ExceptionCode)
		{
			//g::Logger.TraceWarn("Function: [" __FUNCTION__ "] An exception was caught. Code: [" + Text::Format("0x%x", ExceptionCode) + "] Address: [" + Text::Format("0x%x", this) + "]");
		});

	return Result;
}

Media* HistoryMessage::GetMedia()
{
	return *(Media**)((ULONG_PTR)this + g::Offsets::Media);
}

BOOLEAN HistoryMessage::IsSticker()
{
	if (Media* pMedia = GetMedia()) {
		if (DocumentData* pData = pMedia->GetDocument()) {
			return pData->IsSticker();
		}
	}
	return FALSE;
}

BOOLEAN HistoryMessage::IsLargeEmoji()
{
	// if it's a LargeEmoji, [Item->Media] is nullptr, and [Item->MainView->Media] isn't nullptr.
	// if it's a Video, then [Item->Media] isn't nullptr.

	Media* pMedia = GetMedia();
	if (pMedia != NULL) {
		return FALSE;
	}

	HistoryViewElement* pMainView = GetMainView();
	if (pMainView == NULL) {
		return FALSE;
	}

	return pMainView->GetMedia() != NULL;
}

HistoryViewElement* HistoryMessage::GetMainView()
{
	return *(HistoryViewElement**)((ULONG_PTR)this + g::Offsets::MainView);
}

QtString* HistoryMessage::GetTimeText()
{
	return (QtString*)((ULONG_PTR)this + g::Offsets::TimeText);
}

INT HistoryMessage::GetTimeWidth()
{
	return *(INT*)((ULONG_PTR)this + g::Offsets::TimeWidth);
}
void HistoryMessage::SetTimeWidth(INT Value)
{
	*(INT*)((ULONG_PTR)this + g::Offsets::TimeWidth) = Value;
}

//////////////////////////////////////////////////

namespace File
{
	string			GetCurrentFilePathNameA()
	{
		CHAR Buffer[MAX_PATH] = { 0 };
		if (GetModuleFileNameA(NULL, Buffer, MAX_PATH) == 0) {
			return "";
		}

		return string(Buffer);
	}

	ULONG			GetCurrentVersion()
	{
		string FilePathName = GetCurrentFilePathNameA();

		DWORD InfoSize = GetFileVersionInfoSizeA(FilePathName.c_str(), NULL);
		if (!InfoSize) {
			return 0;
		}

		unique_ptr<CHAR[]> Buffer(new CHAR[InfoSize]);
		if (!GetFileVersionInfoA(FilePathName.c_str(), 0, InfoSize, Buffer.get())) {
			return 0;
		}

		VS_FIXEDFILEINFO* pVsInfo = NULL;
		UINT VsInfoSize = sizeof(VS_FIXEDFILEINFO);
		if (!VerQueryValueA(Buffer.get(), "\\", (PVOID*)&pVsInfo, &VsInfoSize)) {
			return 0;
		}

		string TelegramVersion = Text::Format("%03hu%03hu%03hu", HIWORD(pVsInfo->dwFileVersionMS), LOWORD(pVsInfo->dwFileVersionMS), HIWORD(pVsInfo->dwFileVersionLS));
		return stoul(TelegramVersion);
	}
}

namespace Convert
{
	string			UnicodeToAnsi(const wstring& String)
	{
		string Result;
		INT Length = WideCharToMultiByte(CP_ACP, 0, String.c_str(), (INT)String.length(), NULL, 0, NULL, NULL);
		Result.resize(Length);
		WideCharToMultiByte(CP_ACP, 0, String.c_str(), (INT)String.length(), (CHAR*)Result.data(), Length, NULL, NULL);
		return Result;
	}
}

namespace Text
{
	string			ToLower(const string& String)
	{
		string Result = String;
		transform(Result.begin(), Result.end(), Result.begin(), tolower);
		return Result;
	}

	string			SubReplace(const string& Source, const string& Target, const string& New)
	{
		string Result = Source;
		while (true)
		{
			SIZE_T Pos = Result.find(Target);
			if (Pos == string::npos) {
				break;
			}
			Result.replace(Pos, Target.size(), New);
		}
		return Result;
	}

	vector<string>	SplitByFlag(const string& Source, const string& Flag)
	{
		vector<string> Result;
		SIZE_T BeginPos = 0, EndPos = Source.find(Flag);

		while (EndPos != string::npos)
		{
			Result.emplace_back(Source.substr(BeginPos, EndPos - BeginPos));

			BeginPos = EndPos + Flag.size();
			EndPos = Source.find(Flag, BeginPos);
		}

		if (BeginPos != Source.length()) {
			Result.emplace_back(Source.substr(BeginPos));
		}

		return Result;
	}

	string			Format(const CHAR* Format, ...)
	{
		va_list VaList;
		CHAR Buffer[0x200] = { 0 };

		va_start(VaList, Format);
		vsprintf_s(Buffer, Format, VaList);
		va_end(VaList);

		return string(Buffer);
	}
}

void loop()
{
	HistoryMessage* pMessage;
	while (1)
	{
		Sleep(1000);

		if (!g::RevokedMessages.empty()) {
			std::lock_guard<std::mutex> Lock(g::Mutex);
			std::set<HistoryMessage*>::iterator it = g::RevokedMessages.begin();
			HistoryMessage* pMessage = *it;
			g::RevokedMessages.erase(pMessage);

			Safe::Except([&]()
				{
					QtString* pTimeText = NULL;
					HistoryMessageEdited* pEdited = pMessage->GetEdited();
					if (pEdited == NULL) {
						// Normal msg
						pTimeText = pMessage->GetTimeText();
					}
					else {
						// Edited msg
						// The edited message time string is not in Item, but is managed by EditedComponent
						pTimeText = pEdited->GetTimeText();
					}

					if (pTimeText->IsEmpty() || pTimeText->Find(g::CurrentMark.Content) != std::wstring::npos) {
						// [Empty] This message isn't the current channel or group. 
						// [Found] This message is marked.
						return;
					}

					// Mark deleted
					std::wstring MarkedTime = g::CurrentMark.Content + pTimeText->GetText();
					pTimeText->Replace(MarkedTime.c_str());

					// Modify width
					HistoryViewElement* pMainView = pMessage->GetMainView();
					pMainView->SetWidth(pMainView->GetWidth() + g::CurrentMark.Width);
					pMessage->SetTimeWidth(pMessage->GetTimeWidth() + g::CurrentMark.Width);

					// The width of the Sticker and LargeEmoji are aligned to the right
					// So we need to modify one more width, otherwise it will cause the message as a whole to move to the left.
					if (pMessage->IsSticker() || pMessage->IsLargeEmoji())
					{
						Media* pMainViewMedia = pMainView->GetMedia();
						if (pMainViewMedia != NULL) {
							pMainViewMedia->SetWidth(pMainViewMedia->GetWidth() + g::CurrentMark.Width);
						}
						else {
							// (For sticker) This may not be possible, but it takes time to prove.
							g::Logger.TraceWarn("Function: [" __FUNCTION__ "] MainView is nullptr. Address: [" + Text::Format("0x%x", pMessage) + "]");
						}
					}

				}, [&](ULONG ExceptionCode)
				{
					g::Logger.TraceWarn("Function: [" __FUNCTION__ "] An exception was caught. Code: [" + Text::Format("0x%x", ExceptionCode) + "] Address: [" + Text::Format("0x%x", pMessage) + "]");
				});
		}
	}
}