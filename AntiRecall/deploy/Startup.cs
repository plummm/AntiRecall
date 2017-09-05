using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace AntiRecall.deploy
{
    class Startup
    {
        private static RegistryKey startupKey;
        public static string is_hide;

        public static void init_startup()
        {
            if (is_hide == "1")
                return;

            startupKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string QQName = "QQ2009";
            string MyName = "AntiRecall";
            string MyValue = ShortCut.currentDirectory + @"\AntiRecall.exe";
            if (IsInStartup(QQName))
            {
                MessageBoxResult result = MessageBox.Show(@"检测到QQ自启动，是否将其替换为AntiRecall", @"检测到自启动", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    /*
                    ProcessStartInfo startupChanger = new ProcessStartInfo();
#if DEBUG
                    startupChanger.FileName = @".\..\..\..\StartupChanger\bin\Debug\StartupChanger.exe";
#else
                    startupChanger.FileName = ShortCut.currentDirectory + @"StartupChanger.exe";
#endif
                    startupChanger.UseShellExecute = true;
                    startupChanger.Verb = "runas";
                    startupChanger.Arguments = QQName + " " + MyName + " " + MyValue;

                    if (Process.Start(startupChanger)!= null)
                    {

                    }
                    */
                    DeleteStartup(QQName);
                    CreateStartup(MyName, MyValue);
                }
                if (result == MessageBoxResult.No)
                {
                    MessageBoxResult is_hide_startup_notify = MessageBox.Show(@"以后不再显示此提示？", @"提醒", MessageBoxButton.YesNo, MessageBoxImage.Question);
                    if (is_hide_startup_notify == MessageBoxResult.Yes)
                    {
                        is_hide = "1";
                    }
                }
            }
        }

        private static bool IsInStartup(string KeyName)
        {
            if (startupKey != null)
            {
                Object o = startupKey.GetValue(KeyName);
                if (o != null)
                {
                    return true;
                }
                    
            }
            return false;
        }

        private static void DeleteStartup(string KeyName)
        {
            startupKey.DeleteValue(KeyName);
        }

        private static void CreateStartup(string KeyName, string KeyValue)
        {
            startupKey.SetValue(KeyName, KeyValue);
        }

    }
}
