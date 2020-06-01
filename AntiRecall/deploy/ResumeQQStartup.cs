using System;
using Microsoft.Win32;
using System.Windows.Input;

namespace AntiRecall.deploy
{
    public class ResumeQQStartup : ICommand
    {
        private static RegistryKey startupKey;

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            startupKey = Registry.CurrentUser.OpenSubKey("Software\\Microsoft\\Windows\\CurrentVersion\\Run", true);
            string QQName = "QQ2009";
            string MyName = "AntiRecall";
            if (-1 == Xml.currentElement["Path"].IndexOf("QQ.exe"))
            {
                System.Windows.Forms.MessageBox.Show("未设置QQ安装目录");
                return;
            }
            if (IsInStartup(MyName))
            {
                DeleteStartup(MyName);
                CreateStartup(QQName, "\"" + Xml.antiRElement["Path"] + "\"  /background");
            }
            System.Windows.Forms.MessageBox.Show("已恢复QQ自启动，AntiRecall不再自启动");
        }

        public event EventHandler CanExecuteChanged;

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
