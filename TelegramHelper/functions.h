#pragma once
#include "global.h"

class Attributes {
public:
	size_t virtual GetPatternSize() {
		return 0;
	}

	LPVOID GetFunc(DWORD pos) {
		INT offset = *(INT*)(pos + GetPatternSize() + 1);
		return (LPVOID)(pos + +GetPatternSize() + 5 + offset);
	}

	LPVOID GetFuncAddress(DWORD pos) {
		return (LPVOID)(pos + GetPatternSize());
	}
};

class ApplyEdition : public Attributes {

public:
	/*
	void __cdecl HistoryMessage::applyEdition(HistoryMessage *const this, const MTPDmessage *message)

	00823238 | 1BF6                     | sbb esi,esi                                                                                                 |
	0082323A | 23F0                     | and esi,eax                                                                                                 |
	0082323C | 74 67                    | je telegram.8232A5                                                                                          |
	0082323E | 8B43 08                  | mov eax,dword ptr ds:[ebx+8]                                                                                | history_message.cpp:1066
	00823241 | 81CA 00800000            | or edx,8000                                                                                                 | history_message.cpp:1065
	00823247 | 8953 18                  | mov dword ptr ds:[ebx+18],edx                                                                               |
	0082324A | 8B38                     | mov edi,dword ptr ds:[eax]                                                                                  | history_message.cpp:1066
	0082324C | E8 4F41E8FF              | call <telegram.public: static int __cdecl RuntimeComponent<struct HistoryMessageEdited,class HistoryItem>:: |
	00823251 | 837C87 08 04             | cmp dword ptr ds:[edi+eax*4+8],4                                                                            |
	00823256 | 73 28                    | jae telegram.823280                                                                                         |
	00823258 | E8 4341E8FF              | call <telegram.public: static int __cdecl RuntimeComponent<struct HistoryMessageEdited,class HistoryItem>:: | history_message.cpp:1067
	*/
	const char* pattern = "\x8B\x43\x08\x81\xCA\x00\x80\x00\x00\x89\x53\x18\x8B\x38";
	const size_t patternSize = 14;

	PFAPPLYEDITION fn = NULL;
	void* addr = NULL;

	size_t GetPatternSize() {
		return patternSize;
	}

	ApplyEdition() { };

};

class DestroyMessage : public Attributes {

public:

	/*
	00A10543 | 0F84 89000000            | je telegram.A105D2					  |
	00A10549 | 8B49 0C                  | mov ecx,dword ptr ds:[ecx+C]            | ecx+C:"@8"
	00A1054C | 85C9                     | test ecx,ecx                            |
	00A1054E | 0F84 F0000000            | je telegram.A10644                      |
	00A10554 | 8B71 10                  | mov esi,dword ptr ds:[ecx+10]           |
	00A10557 | 85F6                     | test esi,esi                            |
	00A10559 | 0F84 E5000000            | je telegram.A10644                      |
	00A1055F | 51                       | push ecx                                |
	00A10560 | 8BC4                     | mov eax,esp                             |
	00A10562 | 8908                     | mov dword ptr ds:[eax],ecx              |
	00A10564 | 8BCE                     | mov ecx,esi                             |
	00A10566 | E8 35F91500              | call <telegram.sub_B6FEA0>              |
	00A1056B | 80BE 70010000 00         | cmp byte ptr ds:[esi+170],0             |
	00A10572 | 75 13                    | jne telegram.A10587                     |
	00A10574 | 8D45 0C                  | lea eax,dword ptr ss:[ebp+C]            |
	00A10577 | 8975 0C                  | mov dword ptr ss:[ebp+C],esi            |
	00A1057A | 50                       | push eax                                |
	00A1057B | 8D45 D4                  | lea eax,dword ptr ss:[ebp-2C]           |
	00A1057E | 50                       | push eax                                |
	00A1057F | 8D4D C8                  | lea ecx,dword ptr ss:[ebp-38]           |
	*/
	const char* pattern = "\x0F\x84\xE5\x00\x00\x00\x51\x8B\xC4\x89\x08\x8B\xCE";
	const size_t patternSize = 13;

	void* fn = NULL;
	void* addr = NULL;

	size_t GetPatternSize() {
		return patternSize;
	}


	DestroyMessage() { };

private:

};

class Malloc : public Attributes {
public:
	/*
	01CC82A4 | 41                       | inc ecx                                                                                                     |
	01CC82A5 | 84C0                     | test al,al                                                                                                  |
	01CC82A7 | 75 F9                    | jne telegram.1CC82A2                                                                                        |
	01CC82A9 | 2BCA                     | sub ecx,edx                                                                                                 |
	01CC82AB | 53                       | push ebx                                                                                                    |
	01CC82AC | 56                       | push esi                                                                                                    |
	01CC82AD | 8D59 01                  | lea ebx,dword ptr ds:[ecx+1]                                                                                |
	01CC82B0 | 53                       | push ebx                                                                                                    | std_exception.cpp:29
	01CC82B1 | E8 21AF0000              | call <telegram._malloc>                                                                                     |                                                                             |
	*/
	const char* pattern = "\x41\x84\xC0\x75\xF9\x2B\xCA\x53\x56\x8D\x59\x01\x53";
	const size_t patternSize = 13;

	PFMALLOC fn = NULL;
	void* addr = NULL;

	size_t GetPatternSize() {
		return patternSize;
	}


	Malloc() { };
};

class Free : public Attributes {
public:
	/*
	01CC82C6 | 8B45 0C                  | mov eax,dword ptr ss:[ebp+C]                                                                                | std_exception.cpp:36
	01CC82C9 | 8BCE                     | mov ecx,esi                                                                                                 |
	01CC82CB | 83C4 0C                  | add esp,C                                                                                                   | std_exception.cpp:35
	01CC82CE | 33F6                     | xor esi,esi                                                                                                 | std_exception.cpp:36
	01CC82D0 | 8908                     | mov dword ptr ds:[eax],ecx                                                                                  |
	01CC82D2 | C640 04 01               | mov byte ptr ds:[eax+4],1                                                                                   | std_exception.cpp:37
	01CC82D6 | 56                       | push esi                                                                                                    | std_exception.cpp:38
	01CC82D7 | E8 129C0000              | call <telegram._free>                                                                                       |                                                                                |
	*/
	const char* pattern = "\x33\xF6\x89\x08\xC6\x40\x04\x01\x56";
	const size_t patternSize = 9;

	PFFREE fn = NULL;
	void* addr = NULL;

	size_t GetPatternSize() {
		return patternSize;
	}

	Free() { };
};

class SetText : public Attributes {
public:
	/*
	008212E3 | 68 7C67BF02              | push <telegram.struct TextParseOptions const _defaultOptions>                                                       |
	008212E8 | 50                       | push eax                                                                                                            |
	008212E9 | 68 34C6D603              | push <telegram._defaultTextStyle>                                                                                   |
	008212EE | 8BCF                     | mov ecx,edi                                                                                                         |
	008212F0 | C645 FC 02               | mov byte ptr ss:[ebp-4],2                                                                                           |
	008212F4 | E8 C7388400              | call <telegram.public: void __thiscall Ui::Text::String::setText(struct style::TextStyle const &,class QString cons |
	008212F9 | 8B45 EC                  | mov eax,dword ptr ss:[ebp-14]                                                                                       |
	008212FC | 8B00                     | mov eax,dword ptr ds:[eax]                                                                                          |
	008212FE | 85C0                     | test eax,eax                                                                                                        |
	00821300 | 74 16                    | je telegram.821318                                                                                                  |
	*/
	const char* pattern = "\x50\x68\x34\xC6\xD6\x03\x8B\xCF\xC6\x45\xFC\x02";
	const size_t patternSize = 12;

	PFSETTEXT fn = NULL;
	void* addr = NULL;

	size_t GetPatternSize() {
		return patternSize;
	}

	SetText() { };
};

class QStringFromAscii : public Attributes {
public:
	/*
	007C0910 | 8D45 E4                  | lea eax,dword ptr ss:[ebp-1C]                                                                                       |
	007C0913 | 83CB 02                  | or ebx,2                                                                                                            |
	007C0916 | EB 1A                    | jmp telegram.7C0932                                                                                                 |
	007C0918 | 6A 01                    | push 1                                                                                                              |
	007C091A | 68 D418DB02              | push telegram.2DB18D4                                                                                               |
	007C091F | E8 BCAA1501              | call <telegram.private: static struct QTypedArrayData<unsigned short> * __cdecl QString::fromAscii_helper(char cons |
	007C0924 | 8BD0                     | mov edx,eax                                                                                                         |
	007C0926 | 83C4 08                  | add esp,8                                                                                                           |
	007C0929 | 8955 E8                  | mov dword ptr ss:[ebp-18],edx                                                                                       |
	*/
	const char* pattern = "\x83\xCB\x02\xEB\x1A\x6A\x01\x68\xD4\x18\xDB\x02";
	const size_t patternSize = 12;

	PFQSTRINGFROMASCII fn = NULL;
	void* addr = NULL;

	size_t GetPatternSize() {
		return patternSize;
	}

	QStringFromAscii() { };
};