using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using socks5;
using System.Net;
using System.Diagnostics;
using AntiRecall.deploy;
using AntiRecall.network;
using System.Threading;
using System.Windows.Media;
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
            MenuItem menuItem1 = new MenuItem();
            ContextMenu contextMenu = new ContextMenu();

            menuItem1.Index = 0;
            menuItem1.Text = "退出";
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
            if (Xml.antiRElement["Mode"] == "proxy")
            {
                this.PortItem.Foreground = System.Windows.Media.Brushes.Black;
                this.PortText.Foreground = System.Windows.Media.Brushes.Black;
                this.PortText.IsReadOnly = false;
                this.Explorer.Foreground = System.Windows.Media.Brushes.Black;
                this.Explorer.IsEnabled = true;
            }

            if (Xml.antiRElement["Mode"] == "patch")
            {
                this.PortItem.Foreground = System.Windows.Media.Brushes.Gray;
                this.PortText.Foreground = System.Windows.Media.Brushes.Gray;
                this.Explorer.Foreground = System.Windows.Media.Brushes.Black;
                this.Explorer.IsEnabled = true;
            }

            if (-1 != Xml.antiRElement["QQPath"].IndexOf("QQ.exe"))
            {
                this.Explorer.Content = "一切就绪";
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
            ShortCut.init_shortcut("AntiRecall");
            Xml.init_xml();
            CheckUpdate.init_checkUpdate();
            init_minimize();
            if (Xml.CheckXml())
            {
                Xml.antiRElement["PortText"] = Xml.QueryXml("PortText");
                Xml.antiRElement["QQPath"] = Xml.QueryXml("QQPath");
                Xml.antiRElement["Mode"] = Xml.QueryXml("Mode");
                PortText.Text = Xml.antiRElement["PortText"];
            }
            else
            {
                Xml.CreateXml(Xml.antiRElement);
            }
            ModeCheck();
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

            if (this.Start.IsChecked == false)
            {
                if (Xml.antiRElement["Mode"] == "proxy")
                {
                    proxy.Stop();
                }

                if (Xml.antiRElement["Mode"] == "patch")
                {

                }
            }

            if (this.Start.IsChecked == true)
            {
                port = PortText.Text;

                if (Xml.antiRElement["Mode"] == "proxy")
                {
                    init_socks5();
                    //Startup.init_startup();
                    //Modify xml
                    Xml.antiRElement["PortText"] = PortText.Text;
                    if (!Xml.CheckXml())
                        Xml.CreateXml(Xml.antiRElement);
                    else
                        Xml.ModifyXml(Xml.antiRElement);
                    
                }
                else if (Xml.antiRElement["Mode"] == "patch")
                {
                    Xml.antiRElement["PortText"] = PortText.Text;
                    if (!Xml.CheckXml())
                        Xml.CreateXml(Xml.antiRElement);
                    else
                        Xml.ModifyXml(Xml.antiRElement);
                    var th = new Thread(() => patch_memory.StartPatch());
                    th.Start();
                    
                }
                else
                {
                    System.Windows.MessageBox.Show("请选择一个有效的防撤回模式");
                    return;
                }

                if (-1 != Xml.antiRElement["QQPath"].IndexOf("QQ.exe"))
                {
                    try
                    {
                        Process process = new Process();
                        process.StartInfo.FileName = Xml.antiRElement["QQPath"];
                        //process.StartInfo.WindowStyle = ProcessWindowStyle.Maximized;
                        process.StartInfo.CreateNoWindow = true;
                        process.Start();

                    }
                    catch (Exception)
                    {
                        System.Windows.MessageBox.Show("启动QQ.exe失败，请确认路径正确或手动启动");
                    }
                }
                MinimizeWindow();
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
                Xml.antiRElement["QQPath"] = filepath;

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
            ni.BalloonTipText = "已将AntiRecall最小化到托盘,程序将在后台运行";
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

    }
}
