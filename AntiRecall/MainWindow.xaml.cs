using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Drawing;
using socks5;
using System.Net;
using System.Diagnostics;
using AntiRecall.deploy;
using AntiRecall.network;
using System.Threading;
using System.IO;
using AntiRecall.patch;

namespace AntiRecall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {

        private string port;
        private static NotifyIcon ni;
        public static socks5.Socks5Server proxy;
        public static double count { get; set; }
        public static bool is_recallmodule_load { get; set; }

        private void init_minimize()
        {
            System.Windows.Forms.MenuItem menuItem1 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();

            menuItem1.Index = 0;
            menuItem1.Text = "Exit";
            menuItem1.Click += new System.EventHandler(menuItem1_Click);
            contextMenu.MenuItems.Add(menuItem1);

            ni = new NotifyIcon();
            ni.Text = "一个优雅的防撤回工具";
            ni.ContextMenu = contextMenu;
            ni.Visible = true;
#if DEBUG
            ni.Icon = new Icon("../../Resources/main-blue.ico");
#else
            ShortCut.currentDirectory = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
         
            ni.Icon = new Icon(ShortCut.currentDirectory + "\\Resources\\main-blue.ico");
            
#endif
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {

                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        private void init_socks5()
        {
            if (port!=null)
                proxy = new Socks5Server(IPAddress.Any, Convert.ToInt32(port));
            proxy.PacketSize = 65535;
            proxy.Start();
        }

        private delegate void TextChanger();

        public void ModeCheck()
        {
             if (Xml.currentElement["Mode"] == "proxy")
            {
                this.PortItem.Foreground = System.Windows.Media.Brushes.Black;
                this.PortText.Foreground = System.Windows.Media.Brushes.Black;
                this.PortText.IsReadOnly = false;
                this.Proxy_button.IsChecked = true;
                this.Memory_patch_button.IsChecked = false;
                this.Descript_text.Text = "set proxy for QQ";
                this.Explorer.Foreground = System.Windows.Media.Brushes.Black;
                this.Explorer.IsEnabled = true;
            }

            if (Xml.currentElement["Mode"] == "patch")
            {
                this.PortItem.Foreground = System.Windows.Media.Brushes.Gray;
                this.PortText.Foreground = System.Windows.Media.Brushes.Gray;
                this.PortText.IsReadOnly = true;
                this.Proxy_button.IsChecked = false;
                this.Memory_patch_button.IsChecked = true;
                this.Descript_text.Text = "All set";
                this.Explorer.Foreground = System.Windows.Media.Brushes.Black;
                this.Explorer.IsEnabled = true;
            }

            if (-1 != Xml.currentElement["Path"].IndexOf("exe"))
            {
                this.Explorer.Content = "Ready to go";
            }
        }
        /*
        private void UpdateCount()
        {
            Regex re = new Regex("\\[\\d*\\]");
#if DEBUG
            Console.WriteLine(count);
#endif
            Recall_Text.Text = re.Replace(Recall_Text.Text, "["+Convert.ToString(Math.Ceiling(count / 8))+"]");
        }

        public void ModifyRecallCount()
        {
            Recall_Text.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new TextChanger(UpdateCount));
        }*/

        
        public MainWindow()
        {
            InitializeComponent();
            Xml xml = new Xml();
            ShortCut.init_shortcut("AntiRecall");
            Xml.init_xml();
            CheckUpdate.init_checkUpdate();
            init_minimize();
            ModeCheck();
            PortText.Text = Xml.getPort();
            this.Descript_text.Text = "AntiRecall v" + ShortCut.myVersion;
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

            if (this.Start.IsChecked == false)
            {
                if (Xml.currentElement["Mode"] == "proxy")
                {
                    proxy.Stop();
                }

                if (Xml.currentElement["Mode"] == "patch")
                {

                }
            }

            if (this.Start.IsChecked == true)
            {
                port = PortText.Text;

                if (Xml.currentElement["Mode"] == "proxy")
                {
                    init_socks5();
                    //Startup.init_startup();
                    //Modify xml
                    Xml.currentElement["Port"] = PortText.Text;
                    if (!Xml.CheckXml())
                        Xml.CreateXml(Xml.antiRElement);
                    else
                        Xml.ModifyXml(Xml.currentApp, Xml.currentElement);
                    
                }
                else if (Xml.currentElement["Mode"] == "patch")
                {
                    Xml.currentElement["Port"] = PortText.Text;
                    if (!Xml.CheckXml())
                        Xml.CreateXml(Xml.antiRElement);
                    else
                        Xml.ModifyXml(Xml.currentApp, Xml.currentElement);
                    switch (Xml.currentApp)
                    {
                        case "QQ":
                            QQPatch qqPathcher = new QQPatch("QQ", "im.dll");
                            var qqThread = new Thread(() => qqPathcher.StartPatch());
                            qqThread.Start();
                            break;
                        case "Wechat":
                            WechatPatch wcPathcher = new WechatPatch("Wechat", "wechatwin.dll");
                            var wcThread = new Thread(() => wcPathcher.StartPatch());
                            wcThread.Start();
                            break;
                        default:
                            break;
                    }
                    
                }
                else
                {
                    System.Windows.MessageBox.Show("Choose a valid anti-recall method");
                    this.Start.IsChecked = false;
                    return;
                }

                if (-1 != Xml.currentElement["Path"].IndexOf("exe"))
                {
                    try
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = Xml.currentElement["Path"];
                        //process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                        process.StartInfo.CreateNoWindow = true;
                        process.Start();
                        MinimizeWindow();
                    }
                    catch (Exception)
                    {
                        System.Windows.MessageBox.Show("Fail to boot target application，Please check if it's a correct path");
                        this.Start.IsChecked = false;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show("Cannot detect any executeable application. Please check if it's a correct path");
                    this.Start.IsChecked = false;
                }
            }
        }

        private void Explorer_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog 
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            // Set filter for file extension and default file extension 
            dlg.DefaultExt = ".exe";
            dlg.Filter = "Executable Files|*.exe|All Files|*.*";


            // Display OpenFileDialog by calling ShowDialog method 
            Nullable<bool> result = dlg.ShowDialog();


            // Get the selected file name and display in a TextBox 
            if (result == true)
            {
                // Open document 
                string filepath = dlg.FileName;
                Xml.currentElement["Path"] = filepath;

            }

            
        }

        private void menuItem1_Click(object Sender, EventArgs e)
        {
            ni.Visible = false;
            if (proxy != null)
                proxy.Stop();
            Close();
        }

        private void MinimizeWindow()
        {
            this.Hide();

            ni.BalloonTipTitle = "AntiRecall";
            ni.BalloonTipText = "Minimized Antirecall, running on the background";
            ni.BalloonTipIcon = ToolTipIcon.Info;
            ni.ShowBalloonTip(30000);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                MinimizeWindow();
            base.OnStateChanged(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            ni.Visible = false;
            if (proxy!=null)
                proxy.Stop();
            base.OnClosed(e);
            App.Current.Shutdown();
        }

        private void RadRadialMenu_SelectionChanged(object sender, Telerik.Windows.Controls.RadialMenu.MenuSelectionChangedEventArgs e)
        {

        }

        private void Current_App_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {

        }
    }
}
