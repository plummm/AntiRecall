using System;
using System.Collections.Generic;
using System.Linq;
using socks5;
using socks5.TCP;
using System.Windows;
using System.Windows.Controls;
using System.Threading;
using System.Net;
using System.Windows.Forms;

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
            //QQ
            if (e.Buffer[6] == 0x17 && (e.Count == 137 || e.Count == 121))
            {
                e.Buffer[6] = 0x00;

                MainWindow.count++;
            /*
                if ((MainWindow.count + 7) % 8 ==0)
                {
                    App.Current.Dispatcher.Invoke(
                        (Action)delegate {
                            ((MainWindow)System.Windows.Application.Current.MainWindow).ModifyRecallCount();
                        }
                        );
                        
                }
                */
                
#if DEBUG
                Console.WriteLine("capture qq recall");
#endif
            }

            //Wechat
            Console.WriteLine("packet length {0}", e.Count);
            Console.WriteLine("packet buffer {0}", e.Buffer[2]);
            if ((e.Count == 572 || e.Count == 604) && e.Buffer[3] == 0x02 && e.Buffer[4] == 0x37) {
                e.Buffer[2] = 0x0;
#if DEBUG
                Console.WriteLine("capture wechat recall");
#endif
            }
       
            return;
            
        }

    }
}
