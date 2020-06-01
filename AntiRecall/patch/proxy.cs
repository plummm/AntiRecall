using System;
using Microsoft.Win32;
using System.Windows.Input;
using System.Windows.Media;

namespace AntiRecall.patch
{
    public class proxy : ICommand
    {

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            AntiRecall.deploy.Xml.currentElement["Mode"] = "proxy";
            ((MainWindow)System.Windows.Application.Current.MainWindow).ModeCheck();
        }

        public event EventHandler CanExecuteChanged;
    }
}
