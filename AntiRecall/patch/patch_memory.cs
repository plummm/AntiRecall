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

namespace AntiRecall.patch
{
    public class patch_memory : ICommand
    {
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        public ModuleType myProc;
        public string procName { get; set; }
        public string moduleName { get; set; }

        public struct ModuleType
        {
            public IntPtr StartAddr;
            public int Size;
            public int Pid;
        }
        public static NotifyIcon module_ni;

        [DllImport("kernel32.dll")]
        public static extern IntPtr OpenProcess(
            int dwDesiredAccess, 
            bool bInheritHandle, 
            int dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool ReadProcessMemory(
            int hProcess,
            int lpBaseAddress, 
            byte[] lpBuffer, 
            int dwSize, 
            ref int lpNumberOfBytesRead);

        [DllImport("kernel32.dll")]
        public static extern bool WriteProcessMemory(
            int  hProcess,
            int  lpBaseAddress,
            byte[] lpBuffer,
            int  nSize,
            ref int  lpNumberOfBytesWritten);

        [DllImport("kernel32.dll")]
        public static extern bool CloseHandle(
            int hObject);

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

            System.Threading.Thread.Sleep(30000);
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
                        myProc.Size = myProcessModule.ModuleMemorySize;
                        myProc.Pid = processArray[0].Id;
                        break;
                    }
                }
            }

            return result;
        }

        public int Patch(ModuleType process, byte[] pattern, byte[] replacement, int offset)
        {
            IntPtr pHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Pid);
            int bytesRead = 0;
            int bytesWritten = 0;
            byte[] buffer = new byte[process.Size];
            int position;
            int userAddr;
            byte[] byteToWrite;

            if (ReadProcessMemory((int)pHandle, (int)process.StartAddr, buffer, process.Size, ref bytesRead) == false)
                    return -1;

            position = StartingIndex(buffer, pattern);
            if (position < 0)
                return -1;
            //string str = System.Text.Encoding.Unicode.GetString(buffer);
            //int position = str.IndexOf(single);
            userAddr = (int)process.StartAddr + position + offset;
            byteToWrite = replacement.ToArray();

            if (WriteProcessMemory((int)pHandle, userAddr, byteToWrite, byteToWrite.Length, ref bytesWritten) == false)
                return -1;

            LoadBallon();

            CloseHandle((int)pHandle);
            return 1;
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

        public TelegramPatch() { }

        public TelegramPatch(string name1, string name2)
        {
            procName = name1;
            moduleName = name2;
        }

        public override int doPatch(ModuleType process)
        {
            if (Patch(process, tgPattern, tgReplacement, offset) == -1)
                return -1;
            return 0;
        }
    }
}
