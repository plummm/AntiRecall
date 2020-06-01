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
                        MessageBoxResult result = System.Windows.MessageBox.Show(@"Download successfully，please manually unzip and overwrite the old files", @"Congradulation", MessageBoxButton.OK);
                        if (result == MessageBoxResult.OK)
                        {
                            System.Diagnostics.Process.Start(ShortCut.currentDirectory + @"\\tmp");
                        }
                    }
                    else
                    {
                        MessageBoxResult result = System.Windows.MessageBox.Show(@"Fail to download，please update manually.", @"Download error", MessageBoxButton.OK);
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
                return false;
            }
            newVersion = doc.DocumentElement.GetAttribute("Version");
            url = doc.DocumentElement.GetAttribute("Url");

            if (newVersion.Equals(ShortCut.myVersion))
                return false;

            var t1 = newVersion.Split('.');
            var t2 = ShortCut.myVersion.Split('.');

            if (Convert.ToInt32(t1[0]) > Convert.ToInt32(t2[0]))
                return true;
            else if (Convert.ToInt32(t1[1]) > Convert.ToInt32(t2[1]) &&
                Convert.ToInt32(t1[0]) == Convert.ToInt32(t2[0]))
                return true;
            else if (Convert.ToInt32(t1[2]) > Convert.ToInt32(t2[2]) &&
                Convert.ToInt32(t1[0]) == Convert.ToInt32(t2[0]) &&
                Convert.ToInt32(t1[1]) == Convert.ToInt32(t2[1]))
                return true;


            return false;
        }

        private static bool ShowUpdate()
        {
            MessageBoxResult result = System.Windows.MessageBox.Show(@"New version available，will you download it?", @"Check updates", MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                MessageBoxResult result = System.Windows.MessageBox.Show(@"Fail to download，please update manually.", @"Congradulation", MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                {
                    System.Diagnostics.Process.Start(ShortCut.currentDirectory + @"\\tmp");
                }
            }
            else
            {
                MessageBoxResult result = System.Windows.MessageBox.Show(@"Fail to download，please update manually.", @"Download error", MessageBoxButton.OK);
                if (result == MessageBoxResult.OK)
                    System.Diagnostics.Process.Start("https://github.com/FlyRabbit/AntiRecall/releases");
            }
        }
      
    }
}
