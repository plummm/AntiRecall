using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Drawing;
using System.Net;
using System.Diagnostics;
using AntiRecall.deploy;
using AntiRecall.network;
using System.Threading;
using System.IO;
using AntiRecall.patch;
using System.Collections.Generic;

namespace AntiRecall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {
        private static NotifyIcon ni;
        public static Dictionary<string, patch_memory> instances;
        public static double count { get; set; }
        public static bool is_recallmodule_load { get; set; }

        private void Init_minimize()
        {
            System.Windows.Forms.MenuItem menuItem1 = new System.Windows.Forms.MenuItem();
            System.Windows.Forms.ContextMenu contextMenu = new System.Windows.Forms.ContextMenu();

            menuItem1.Index = 0;
            menuItem1.Text = "Exit";
            menuItem1.Click += new System.EventHandler(MenuItem1_Click);
            contextMenu.MenuItems.Add(menuItem1);

            ni = new NotifyIcon();
            ni.Text = "一个优雅的防撤回工具";
            ni.ContextMenu = contextMenu;
            ni.Visible = true;
#if DEBUG
            ni.Icon = new Icon("../../Resources/main-blue.ico");
#else
            ShortCut.currentDirectory = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            try
            {
                ni.Icon = new Icon(ShortCut.currentDirectory + "\\Resources\\main-blue.ico");
            }
            catch (Exception)
            {
                Console.WriteLine("No ico found");
            }
            
#endif
            ni.DoubleClick +=
                delegate (object sender, EventArgs args)
                {

                    this.Show();
                    this.WindowState = WindowState.Normal;
                };
        }

        private delegate void TextChanger();

        public void ModeCheck()
        {

            if (Xml.currentElement["Mode"] == "patch")
            {
                this.Descript_text.Text = "All set";
                this.Explorer.Foreground = System.Windows.Media.Brushes.Black;
                this.Explorer.IsEnabled = true;
            }

            if (-1 != Xml.currentElement["Path"].IndexOf("exe"))
            {
                this.Explorer.Content = Strings.explorer_ready;
            }
        }
        
        public MainWindow()
        {
            InitializeComponent();
            instances = InitializeInstances();
            Xml xml = new Xml();
            ShortCut.init_shortcut(Strings.title);
            xml.Init_xml();
            CheckUpdate.init_checkUpdate();
            Init_minimize();
            ModeCheck();
            this.Descript_text.Text = Strings.title+" v" + ShortCut.myVersion;
        }

        private Dictionary<string, patch_memory> InitializeInstances()
        {
            Dictionary<string, patch_memory> instances = new Dictionary<string, patch_memory>();
            instances["QQ"] = new QQPatch("QQ", "im.dll");
            instances["Wechat"] = new WechatPatch("Wechat", "wechatwin.dll");
            instances["Telegram"] = new TelegramPatch("Telegram", "telegram.exe");
            return instances;
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {

            if (this.Start.IsChecked == false)
            {
                Xml.currentElement["Launch"] = Strings.launch_stopped;

                if (Xml.currentElement["Mode"] == "patch")
                {

                }
            }

            if (this.Start.IsChecked == true)
            {
                Xml.currentElement["Launch"] = Strings.launch_started;

                if (Xml.currentElement["Mode"] == "patch")
                {
                    Xml.ModifyXml(Xml.currentApp, Xml.currentElement);
                    var th = new Thread(() => instances[Xml.currentApp].StartPatch());
                    th.Start();
                }
                else
                {
                    System.Windows.MessageBox.Show(Strings.invalid_method, Strings.warning);
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
                        System.Windows.MessageBox.Show(Strings.incorrect_target_path, Strings.warning);
                        this.Start.IsChecked = false;
                    }
                }
                else
                {
                    System.Windows.MessageBox.Show(Strings.invalid_target_path, Strings.warning);
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

        private void MenuItem1_Click(object Sender, EventArgs e)
        {
            ni.Visible = false;
            Close();
        }

        private void MinimizeWindow()
        {
            this.Hide();

            ni.BalloonTipTitle = Strings.title;
            ni.BalloonTipText = Strings.minimized;
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
