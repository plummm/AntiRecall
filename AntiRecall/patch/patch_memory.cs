using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Input;
using System.Runtime.CompilerServices;
using AntiRecall.deploy;

/*
 * char, INT8, SBYTE, CHARâ€ 	                                        8-bit signed integer	                System.SByte
 * short, short int, INT16, SHORT	                                    16-bit signed integer	                System.Int16
 * int, long, long int, INT32, LONG32, BOOLâ€ , INT	                    32-bit signed integer	                System.Int32
 * __int64, INT64, LONGLONG	                                            64-bit signed integer	                System.Int64
 * unsigned char, UINT8, UCHARâ€ , BYTE	                                8-bit unsigned integer	                System.Byte
 * unsigned short, UINT16, USHORT, WORD, ATOM, WCHARâ€ , __wchar_t	    16-bit unsigned integer	                System.UInt16
 * unsigned, unsigned int, UINT32, ULONG32, DWORD32, ULONG, DWORD, UINT	32-bit unsigned integer	                System.UInt32
 * unsigned __int64, UINT64, DWORDLONG, ULONGLONG	                    64-bit unsigned integer	                System.UInt64
 * float, FLOAT	                                                        Single-precision floating point	        System.Single
 * double, long double, DOUBLE	                                        Double-precision floating point	        System.Double
 */
using HANDLE = System.IntPtr;
using DWORD = System.UInt32;
using BOOL = System.Boolean;
using SIZE_T = System.UInt32;
using LPDWORD = System.UInt32;

//System.IntPtr
using LPVOID = System.IntPtr;
using FARPROC = System.IntPtr;
using HMODULE = System.IntPtr;

//System.String
using LPCTSTR = System.String;
using LPSTR = System.String;
using LPCSTR = System.String;
using PCSTR = System.String;
using PSTR = System.String;
using LPCWSTR = System.String;
using LPWSTR = System.String;
using PCWSTR = System.String;
using PWSTR = System.String;
using System.Text;

namespace AntiRecall.patch
{
    public class patch_memory : ICommand
    {
        //mask
        const uint PROCESS_ALL_ACCESS = 0x1F0FFF;
        const uint MEM_COMMIT = 0x00001000;
        const uint PAGE_READWRITE = 0x04;
        const uint INFINITE = 0xFFFFFFFF;

        public ModuleType myProc;
        public string procName { get; set; }
        public string moduleName { get; set; }

        public struct ModuleType
        {
            public IntPtr StartAddr;
            public SIZE_T Size;
            public uint Pid;
        }
        public static NotifyIcon module_ni;

        [DllImport("kernel32.dll")]
        public static extern HANDLE OpenProcess(
            DWORD dwDesiredAccess,
            BOOL bInheritHandle,
            DWORD dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern LPVOID VirtualAllocEx(
            HANDLE hProcess,
            LPVOID lpAddress,
            SIZE_T dwSize,
            DWORD flAllocationType,
            DWORD flProtect);

        [DllImport("kernel32.dll")]
        public static extern BOOL WriteProcessMemory(
            HANDLE hProcess,
            LPVOID lpBaseAddress,
            byte[] lpBuffer,
            SIZE_T nSize,
            ref SIZE_T lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll")]
        public static extern BOOL WriteProcessMemory(
            HANDLE hProcess,
            LPVOID lpBaseAddress,
            UInt16[] lpBuffer,
            SIZE_T nSize,
            ref SIZE_T lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll")]
        public static extern BOOL ReadProcessMemory(
            HANDLE hProcess,
            LPVOID lpBaseAddress,
            byte[] lpBuffer,
            SIZE_T dwSize,
            ref SIZE_T lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern FARPROC GetProcAddress(
            HMODULE hModule,
            LPCSTR lpProcName);

        [DllImport("kernel32.dll")]
        public static extern HMODULE GetModuleHandle(
            LPCTSTR lpModuleName);

        [DllImport("kernel32.dll")]
        public static extern DWORD WaitForSingleObject(
            HANDLE hHandle,
            DWORD dwMilliseconds);

        [DllImport("kernel32.dll")]
        public static extern HANDLE CreateRemoteThread(
            HANDLE hProcess,
            IntPtr lpThreadAttributes,
            SIZE_T dwStackSize,
            IntPtr lpStartAddress,
            int lpParameter,
            DWORD dwCreationFlags,
            LPDWORD lpThreadId
        );

        [DllImport("kernel32.dll")]
        public static extern BOOL CloseHandle(
            HANDLE hObject);

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public virtual void Execute(object parameter)
        {
            AntiRecall.deploy.Xml.currentElement["Mode"] = "patch";
            ((MainWindow)System.Windows.Application.Current.MainWindow).ModeCheck();
        }

        public event EventHandler CanExecuteChanged;

        public SortedDictionary<string, string> BasicInfo()
        {
            SortedDictionary<string, string> dict = new SortedDictionary<string, string>
            {
                ["Name"] = procName,
                ["Port"] = "",
                ["Path"] = "",
                ["Mode"] = "",
                ["Descript"] = Strings.title
            };
            return dict;
        }
        public int StartingIndex(byte[] x, byte[] y)
        {
            try
            {
                IEnumerable<int> index = Enumerable.Range(0, x.Length - y.Length + 1);
                for (int i = 0; i < y.Length; i++)
                {
                    index = index.Where(n => x[n + i] == y[i]).ToArray();
                }
                return index.First();
            }
            catch
            {
                return -2;
            }
        }

        public void disposeBallon(System.Windows.Forms.MouseEventHandler value)
        {
            module_ni.Visible = false;
        }

        public void LoadBallon()
        {
            module_ni = new NotifyIcon();
            module_ni.Icon = SystemIcons.Exclamation;
            module_ni.Visible = true;
            module_ni.BalloonTipTitle = Strings.title;
            module_ni.BalloonTipText = Strings.loaded_module;
            module_ni.BalloonTipIcon = ToolTipIcon.Info;
            module_ni.ShowBalloonTip(30000);
            module_ni.Visible = false;
            module_ni.Dispose();
        }

        public virtual int doPatch(ModuleType proc)
        {
            return 0;
        }
        public void StartPatch()
        {
            int is_running = 0;
            int result = -1;

#if DEBUG
            System.Threading.Thread.Sleep(2);
#else
            System.Threading.Thread.Sleep(30000);
#endif
            is_running = FindProcess();
            if (is_running == 1)
            {
                result = doPatch(myProc);
                if (result == -1)
                {
                    System.Windows.MessageBox.Show(Strings.failed_loaded_module);
                }
            }
        }

        private int FindProcess()
        {
            int result = 0;
            ProcessModule myProcessModule;
            ProcessModuleCollection PMCollection;
            Process[] processArray;

            processArray = Process.GetProcessesByName(procName);
            if (processArray.Length != 0)
            {
                result += 1;

                PMCollection = processArray[0].Modules;
                for (int i = 0; i < PMCollection.Count; i++)
                {
                    myProcessModule = PMCollection[i];
                    if (myProcessModule.ModuleName.ToLower().Equals(moduleName))
                    {
                        myProc.StartAddr = myProcessModule.BaseAddress;
                        myProc.Size = (SIZE_T)myProcessModule.ModuleMemorySize;
                        myProc.Pid = (DWORD)processArray[0].Id;
                        break;
                    }
                }
            }

            return result;
        }

        public int Patch(ModuleType process, byte[] pattern, byte[] replacement, int offset)
        {
            HANDLE pHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Pid);
            SIZE_T bytesRead = 0;
            SIZE_T bytesWritten = 0;
            byte[] buffer = new byte[(int)process.Size];
            int position;
            int userAddr;
            byte[] byteToWrite;

            if (ReadProcessMemory(pHandle, process.StartAddr, buffer, process.Size, ref bytesRead) == false)
                    return -1;

            position = StartingIndex(buffer, pattern);
            if (position < 0)
                return -1;
            //string str = System.Text.Encoding.Unicode.GetString(buffer);
            //int position = str.IndexOf(single);
            userAddr = (int)process.StartAddr + position + offset;
            byteToWrite = replacement.ToArray();
            //IntPtr pByteToWrite = ByteArray2IntPtr(byteToWrite);

            if (WriteProcessMemory(pHandle, new IntPtr(userAddr), byteToWrite, (SIZE_T)byteToWrite.Length, ref bytesWritten) == false)
                return -1;

            LoadBallon();

            CloseHandle(pHandle);
            return 1;
        }

        public bool InjectDll(ModuleType process, LPCTSTR szDllPath)
        {
            IntPtr pThreadProc;
            HANDLE hThread, hProcess, pRemoteBuf;
            SIZE_T bytesWritten = 0;
            DWORD dwBufSize = (DWORD)(szDllPath.Length + 1) * 2;
            int buf;

            //wchar in C
            UInt16[] wDllPath = szDllPath.ToCharArray().Select(x => (UInt16)x).ToArray();


            hProcess = OpenProcess(PROCESS_ALL_ACCESS, false, process.Pid);
            if (hProcess == IntPtr.Zero)
            {
                return false;
            }
            pRemoteBuf = VirtualAllocEx(hProcess, IntPtr.Zero, dwBufSize, MEM_COMMIT, PAGE_READWRITE);

            WriteProcessMemory(hProcess, pRemoteBuf, wDllPath, dwBufSize, ref bytesWritten);
            pThreadProc = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryW");
            unsafe
            {
                buf = pRemoteBuf.ToInt32();
            }
            hThread = CreateRemoteThread(hProcess, IntPtr.Zero, 0, pThreadProc, buf, 0, 0);
            WaitForSingleObject(hThread, INFINITE);
            CloseHandle(hThread);
            CloseHandle(hProcess);

            return true;
        }

        private IntPtr ByteArray2IntPtr(byte[] array)
        {
            IntPtr ptr = Marshal.AllocHGlobal(array.Length);
            Marshal.Copy(array, 0, ptr, array.Length);
            return ptr;
        }

        private byte[] IntPtr2ByteArray(IntPtr ptr, int size)
        {
            byte[] managedArray = new byte[size];
            Marshal.Copy(ptr, managedArray, 0, size);
            return managedArray;
        }
    }

    public class QQPatch : patch_memory
    {
        private static readonly byte[] chat = { (byte)'\x81', (byte)'\x7d', (byte)'\x0c', (byte)'\x8a', (byte)'\x00' };
        private static readonly byte[] groupChat = { (byte)'\x80', (byte)'\x7D', (byte)'\xFF', (byte)'\x11', (byte)'\x0f' };
        private static readonly byte[] chatReplacement = { (byte)'\x84' };
        private static readonly int chatOffset = -5;
        private static readonly int groupChatOffset = 5;

        public QQPatch() { }

        public QQPatch(string name1, string name2) 
        {
            procName = name1;
            moduleName = name2;
        }

        public override int doPatch(ModuleType process)
        {
            if (Patch(process, chat, chatReplacement, chatOffset) == -1 ||
                Patch(process, groupChat, chatReplacement, groupChatOffset) == -1)
                return -1;
            return 0;
        }

    }
    public class WechatPatch : patch_memory
    {
        private static readonly byte[] wechatPattern = { (byte)'\x83', (byte)'\xC4', (byte)'\x14', (byte)'\x84', (byte)'\xC0', (byte)'\x74', (byte)'\x7D' };
        private static readonly byte[] chatReplacement = { (byte)'\x7d' };
        private static readonly int offset = 5;

        public WechatPatch() { }

        public WechatPatch(string name1, string name2)
        {
            procName = name1;
            moduleName = name2;
        }

        public override int doPatch(ModuleType process)
        {
            if (Patch(process, wechatPattern, chatReplacement, offset) == -1)
                return -1;
            return 0;
        }
    }

    public class TelegramPatch : patch_memory
    {
        private static readonly byte[] tgPattern = { (byte)'\x0F', (byte)'\x84', (byte)'\xE5', (byte)'\x00', (byte)'\x00', (byte)'\x00', (byte)'\x51', (byte)'\x8B', (byte)'\xC4', (byte)'\x89', (byte)'\x08', (byte)'\x8B', (byte)'\xCE' };
        private static readonly byte[] tgReplacement = { (byte)'\x90', (byte)'\x90', (byte)'\x90', (byte)'\x90', (byte)'\x90' };
        private static readonly int offset = 13;
        private static readonly string dllName = "\\TelegramHelper.dll";

        public TelegramPatch() { }

        public TelegramPatch(string name1, string name2)
        {
            procName = name1;
            moduleName = name2;
        }

        public override int doPatch(ModuleType process)
        {
            Patch(process, tgPattern, tgReplacement, offset);
            InjectDll(process, ShortCut.currentDirectory + dllName);
            return 0;
        }
    }
}
