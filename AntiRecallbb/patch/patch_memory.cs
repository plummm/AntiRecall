using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace AntiRecall.patch
{
    class patch_memory
    {
        const int PROCESS_ALL_ACCESS = 0x1F0FFF;
        private static byte[] single = { (byte)'\x81', (byte)'\x7d', (byte)'\x0c', (byte)'\x8a', (byte)'\x00' };
        private static byte[] group = { (byte)'\x80', (byte)'\x7D', (byte)'\xFF', (byte)'\x11', (byte)'\x0f' };
        private struct ModuleType
        {
            public IntPtr StartAddr;
            public int Size;
            public int Pid;
        }
        private static ModuleType pTIM;
        private static ModuleType pQQ;
        private static NotifyIcon module_ni;

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

        public static int StartingIndex(byte[] x, byte[] y)
        {
            IEnumerable<int> index = Enumerable.Range(0, x.Length - y.Length + 1);
            for (int i = 0; i < y.Length; i++)
            {
                index = index.Where(n => x[n + i] == y[i]).ToArray();
            }
            return index.First();
        }

        private void disposeBallon(MouseEventHandler value)
        {
            module_ni.Visible = false;
        }

        private static void LoadBallon()
        {
            module_ni = new NotifyIcon();
            module_ni.Icon = SystemIcons.Exclamation;
            module_ni.Visible = true;
            module_ni.BalloonTipTitle = "AntiRecall";
            module_ni.BalloonTipText = "防撤回模块已成功加载";
            module_ni.BalloonTipIcon = ToolTipIcon.Info;
            module_ni.ShowBalloonTip(30000);
            module_ni.Visible = false;
            module_ni.Dispose();
        }

        public static void StartPatch()
        {
            int is_running = FindProcess();
            int resultQQ = -1;
            int resultTim = -1;
            if ((is_running & 1) == 1) //QQ.exe
            {
                resultQQ = Patch(pQQ);
            }
            if ((is_running >> 1 & 1) == 1) //Tim.exe
            {
                resultTim = Patch(pTIM);
            }

            if (resultQQ == 0)
            {
                System.Windows.MessageBox.Show("QQ防撤回补丁加载失败，请关闭杀毒软件后重试。");
            }
            if (resultTim == 0)
            {
                System.Windows.MessageBox.Show("Tim防撤回补丁加载失败，请关闭杀毒软件后重试。");
            }
        }

        private static int FindProcess()
        {
            int result = 0;
            ProcessModule myProcessModule;
            ProcessModuleCollection PMCollection;
            Process[] processArray;

            processArray = Process.GetProcessesByName("QQ");
            if (processArray.Length != 0)
            {
                result += 1;

                PMCollection = processArray[0].Modules;
                for (int i = 0; i < PMCollection.Count; i++)
                {
                    myProcessModule = PMCollection[i];
                    if (myProcessModule.ModuleName.Equals("IM.dll"))
                    {
                        pQQ.StartAddr = myProcessModule.BaseAddress;
                        pQQ.Size = myProcessModule.ModuleMemorySize;
                        pQQ.Pid = processArray[0].Id;
                        break;
                    }
                }
            }

            processArray = Process.GetProcessesByName("TIM");
            if (processArray.Length != 0)
            {
                result += 2;

                PMCollection = processArray[0].Modules;
                for (int i = 0; i < PMCollection.Count; i++)
                {
                    myProcessModule = PMCollection[i];
                    if (myProcessModule.ModuleName.Equals("IM.dll"))
                    {
                        pTIM.StartAddr = myProcessModule.BaseAddress;
                        pTIM.Size = myProcessModule.ModuleMemorySize;
                        pTIM.Pid = processArray[0].Id;
                        break;
                    }
                }
            }
           

            return result;
        }

        private static int Patch(ModuleType process)
        {
            IntPtr pHandle = OpenProcess(PROCESS_ALL_ACCESS, false, process.Pid);
            int userAddr;
            int groupAddr;
            int bytesRead = 0;
            int bytesWritten = 0;
            byte[] buffer = new byte[process.Size];
            byte[] byteToWrite = new byte[1];

            if (ReadProcessMemory((int)pHandle, (int)process.StartAddr, buffer, process.Size, ref bytesRead) == false)
                return -1;

            int position = StartingIndex(buffer, single);
            //string str = System.Text.Encoding.Unicode.GetString(buffer);
            //int position = str.IndexOf(single);
            userAddr = (int)process.StartAddr + position - 5;
            if ((char)buffer[position - 5] == '\x85')
                byteToWrite[0] = (byte)'\x84';
            else
                byteToWrite[0] = (byte)'\x85';
            position = StartingIndex(buffer, group);
            groupAddr = (int)process.StartAddr + position + 5;

            if (WriteProcessMemory((int)pHandle, userAddr, byteToWrite, 1, ref bytesWritten) == false)
                return -1;

            if (WriteProcessMemory((int)pHandle, groupAddr, byteToWrite, 1, ref bytesWritten) == false)
                return -1;

            if (byteToWrite[0] == '\x84')
                LoadBallon();

            CloseHandle((int)pHandle);
            return 1;
        }
    }
}
