#include "myFunc.h"

VOID TagBadge(HistoryMessage* pMessage)
{

	//QtString* badge = (QtString*)g::qStringFromAscii.fn("deleted", 8);
	//Text::String* _messageBadge = (Text::String*)pMessage->GetMessageBadge();
	//QtString** a = (QtString**)((DWORD)_messageBadge + 0xc);
	//*a = badge;
	//QtString* badge = (QtString *)_messageBadge->GetText();
	//std::wstring MarkedTime = g::CurrentMark.Content;
	//badge->Replace(MarkedTime.c_str());
	
	//__asm
	//{
	//	mov ecx, _messageBadge
	//}
	//g::setText.fn((void*)0x3D6C634, &badge, (void*)0x2BF677C);
	
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
	
}

VOID HandleDeleteMessage(HistoryMessage* pMessage)
{
	Safe::Except([&]()
	{
		if (!pMessage->toHistoryMessage()) {
			return;
		}

		
		QtString* pTimeText = pMessage->GetTimeText();
		if (!pTimeText->IsValidTime()) {
			g::Logger.TraceWarn("A bad TimeText. Address: [" + Text::Format("0x%x", pMessage) + "] Content: [" + Convert::UnicodeToAnsi(pTimeText->GetText()) + "]");
			return;
		}
		

		std::lock_guard<std::mutex> Lock(g::Mutex);
		//g::RevokedMessages.insert(pMessage);
		TagBadge(pMessage);

	}, [](ULONG ExceptionCode)
	{
		g::Logger.TraceWarn("Function: [" __FUNCTION__ "] An exception was caught. Code: [" + Text::Format("0x%x", ExceptionCode) + "]");
	});
}

VOID HandleFree(void* block)
{
	std::unique_lock<std::mutex> Lock(g::Mutex);
	// When we delete a msg by ourselves, Telegram will free this memory block.
	// So, we will earse this msg from the vector.

	g::RevokedMessages.erase((HistoryMessage*)block);
	Lock.unlock();

	g::fnFree.fn(block);
}