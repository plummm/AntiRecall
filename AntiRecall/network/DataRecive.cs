using System;
using System.Collections.Generic;
using System.Linq;
using socks5;
using socks5.HTTP;
using socks5.TCP;
using System.Windows;
using System.Windows.Controls;
using System.Threading;

namespace AntiRecall.network
{
    class DataRecive : socks5.Plugin.DataHandler
    {
        public override bool OnStart()
        {
            return true;
        }

        private bool enabled = true;
        public override bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }


        public override void OnClientDataReceived(object sender, DataEventArgs e)
        {
            return;
        }

        public override void OnServerDataReceived(object sender, DataEventArgs e)
        {
            if (e.Count == 137 && e.Buffer[6]==0x17)
            {
#if DEBUG
                Console.WriteLine("capture recall");
#endif
                e.Buffer[6] = 0x00;
                
                MainWindow.count++;
                if ((MainWindow.count + 7) % 8 ==0)
                {
                    MainWindow window = new MainWindow();
                    Thread recallCount = new Thread(window.ModifyRecallCount);
                    recallCount.Start();
                    
                }
            }
            
            return;
        }
    }
}
