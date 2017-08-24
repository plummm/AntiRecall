using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Forms;
using System.Drawing;
using socks5;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;

namespace AntiRecall
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    
    public partial class MainWindow : Window
    {

        private string port;
        public static socks5.Socks5Server proxy;
        public static double count { get; set; }

        private void init_minimize()
        {
            NotifyIcon ni = new NotifyIcon();
            ni.Icon = new Icon("../../Resources/main.ico");
            ni.Visible = true;
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

        private void UpdateCount()
        {
            Regex re = new Regex("[\\d*?]");
            Console.WriteLine(count);
            Recall_Text.Text = re.Replace(Recall_Text.Text, Convert.ToString(Math.Ceiling(count / 8)));
        }

        public void ModifyRecallCount()
        {
            while (true)
            Recall_Text.Dispatcher.Invoke(
                System.Windows.Threading.DispatcherPriority.Normal,
                new TextChanger(UpdateCount));
        }

        
        public MainWindow()
        {
            InitializeComponent();

            init_minimize();
            Thread recallCount = new Thread(ModifyRecallCount);
            recallCount.Start();
        }


        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            // CoolButton Clicked! Let's show our InputBox.
            //InputBox.Visibility = System.Windows.Visibility.Visible;
            port = PortText.Text;
            Start.IsEnabled = false;
            Start.Content = "正在监听";
            init_socks5();
        }

        protected override void OnStateChanged(EventArgs e)
        {
            if (WindowState == System.Windows.WindowState.Minimized)
                this.Hide();

            base.OnStateChanged(e);
        }

    }
}
