using System;
using System.Collections.Generic;
using System.Linq;
using socks5;
using socks5.HTTP;
using socks5.TCP;
using System.Windows;
using System.Windows.Controls;

namespace AntiRecall.network
{
    class DataRecive : socks5.Plugin.DataHandler
    {
        public override bool OnStart()
        {
            return true;
        }

        private Object thisLock = new Object();
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

        private bool Is_Recall(byte[] buffer)
        {
            byte[] key = { 0x00, 0x89, 0x02, 0x37, 0x21, 0x00, 0x17 };
            for (int i = 0; i < 7; i++)
            {
                if (key[i] != buffer[i])
                    return false;
            }
            return true;
        }

        public override void OnClientDataReceived(object sender, DataEventArgs e)
        {
            return;
        }

        public override void OnServerDataReceived(object sender, DataEventArgs e)
        {
            if (e.Count == 137 && e.Buffer[6]==0x17)
            {
                e.Buffer[6] = 0x00;
                MainWindow.count++;
            }
            return;
        }
    }
}
