using System;
using Microsoft.Win32;
using System.Windows.Input;

namespace AntiRecall.deploy
{
    public class ReplaceQQStartup : ICommand
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
            string MyValue = "\"" + ShortCut.currentDirectory + @"\AntiRecall.exe" + "\"";
            if (IsInStartup(QQName))
            {
                DeleteStartup(QQName);   
            }
            if (!IsInStartup(MyName))
            {
                CreateStartup(MyName, MyValue);
            }
            System.Windows.Forms.MessageBox.Show("已将QQ自启动替换为AntiRecall");
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
