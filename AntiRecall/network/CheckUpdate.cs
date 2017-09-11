using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Windows;
using AntiRecall.deploy;
using System.Net;
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;

namespace AntiRecall.network
{
    class CheckUpdate
    {
        private static string newVersion;
        private static string url;
        private static WebClient client;

        public static void init_checkUpdate()
        {
            Thread thread = new Thread(() => StartUpdate());
            thread.Start();
        }

        private static void StartUpdate()
        {
            if (CheckNewVersion())
            {
                if (ShowUpdate())
                {
                    if (DownloadNewVersion())
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show(@"下载成功，请手动解压覆盖源文件", @"大吉大利，今晚吃鸡", MessageBoxButton.OK);
                        if (result == MessageBoxResult.OK)
                        {
                            System.Diagnostics.Process.Start(ShortCut.currentDirectory + @"\\tmp");
                        }
                    }
                    else
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show(@"下载失败，请手动更新。", @"错误", MessageBoxButton.OK);
                        if (result == MessageBoxResult.OK)
                            System.Diagnostics.Process.Start("https://github.com/FlyRabbit/AntiRecall/releases");
                    }
                }
            }
        }
    


        private static bool CheckNewVersion()
        {
            XmlDocument doc = new XmlDocument();
            try
            {

                doc.Load("https://etenal.me/wp-content/uploads/AntiRecall/newversion.xml");
            }
            catch
            {
                //System.Windows.MessageBox.Show(@"检查更新失败，请检查计算机是否连接到互联网。");
            }
            newVersion = doc.DocumentElement.GetAttribute("Version");
            url = doc.DocumentElement.GetAttribute("Url");
            
            return (!newVersion.Equals(ShortCut.myVersion));
        }

        private static bool ShowUpdate()
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(@"检测到新版本，是否下载更新？", @"检查更新", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
                return true;
            else
                return false;
        }

        private static bool DownloadNewVersion()
        {
            if (!System.IO.Directory.Exists(ShortCut.currentDirectory + @"\\tmp"))
                System.IO.Directory.CreateDirectory(ShortCut.currentDirectory + @"\\tmp");

            try
            {
                /*
                Thread thread = new Thread(() => {
                    client = new WebClient();
                    client.DownloadFileCompleted += DownloadCompleted;
                    client.DownloadFileAsync(new Uri(url), @".\\tmp\\AntiRecall.zip");
                });
                thread.Start();
                */
                client = new WebClient();
                client.DownloadFile(url, ".\\tmp\\AntiRecall.zip");
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private static void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            long size = new System.IO.FileInfo(@".\\tmp\\AntiRecall.zip").Length;
            if (size != 0)
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(@"下载成功，请手动解压覆盖源文件", @"大吉大利，今晚吃鸡", MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                {
                    System.Diagnostics.Process.Start(ShortCut.currentDirectory + @"\\tmp");
                }
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(@"下载失败，请手动更新。", @"错误", MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                    System.Diagnostics.Process.Start("https://github.com/FlyRabbit/AntiRecall/releases");
            }
        }
      
    }
}
