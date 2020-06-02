using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml;
using AntiRecall.deploy;

namespace AntiRecall.deploy
{
    public class Xml : ICommand
    {
        public static string QQ_ori_path;
        public static SortedDictionary<string, SortedDictionary<string, string>> antiRElement;
        public static SortedDictionary<string, string> currentElement;
        public static string currentApp;
        private static string[] namelist = { "QQ", "Wechat", "Telegram" };

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public virtual void Execute(object parameter)
        {
            string name = (string)((Telerik.Windows.Controls.RadRadialMenuItem)parameter).Header;
            SwitchApp(name);
        }

        public event EventHandler CanExecuteChanged;
        public void Init_xml()
        {
            bool update = false;
            currentApp = "Telegram";
            antiRElement = new SortedDictionary<string, SortedDictionary<string, string>>();
            if (CheckXml())
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(ShortCut.currentDirectory + @"\setting.xml");
                XmlNode root = doc.DocumentElement.SelectSingleNode("/Setting");
                currentApp = root.Attributes["Primary"].Value;
                XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Setting/App");
                foreach (XmlNode node in nodes)
                {
                    string appName = node.Attributes["Name"].Value;
                    var app = new SortedDictionary<string, string>();
                    app["Name"] = node.Attributes["Name"].Value;
                    app["Port"] = node.Attributes["Port"].Value;
                    app["Path"] = node.Attributes["Path"].Value;
                    app["Mode"] = node.Attributes["Mode"].Value;
                    app["Descript"] = node.Attributes["Descript"].Value;
                    antiRElement[appName] = app;
                }
            }

            foreach (string name in namelist)
            {
                if (!antiRElement.ContainsKey(name))
                {
                    antiRElement[name] = MainWindow.instances[name].BasicInfo();
                    update = true;
                }
            }
            if (update)
            {
                System.IO.File.Delete(ShortCut.currentDirectory + @"\setting.xml");
                CreateXml(antiRElement);
            }

            currentElement = antiRElement[currentApp];
            SwitchApp(currentApp);
            _ = (MainWindow)System.Windows.Application.Current.MainWindow;
        }
        
        public void SwitchApp(string name)
        {
            MainWindow window = (MainWindow)System.Windows.Application.Current.MainWindow;
            currentApp = name;
            currentElement = antiRElement[currentApp];
            window.Current_App.Text = currentApp;
            if (currentElement["Mode"] == "proxy")
            {
                window.Proxy_button.IsChecked = true;
            } 
            else
            {
                window.Proxy_button.IsChecked = false;
            }

            if (currentElement["Mode"] == "patch")
            {
                window.Memory_patch_button.IsChecked = true;
            }
            else
            {
                window.Memory_patch_button.IsChecked = false;
            }

            if (-1 != currentElement["Path"].IndexOf("exe"))
            {
                window.Explorer.Content = Strings.explorer_ready;
            }
            else
            {
                window.Explorer.Content = Strings.explorer_hold;
            }
            window.PortText.Text = currentElement["Port"];
        }

        public string GetPort()
        {
            foreach (var entry in antiRElement)
            {
                var app = entry.Value;
                return app["Port"];
            }
            return null;
        }

        public string GetDescription()
        {
            foreach (var entry in antiRElement)
            {
                var app = antiRElement[entry.Key];
                return app["Descript"];
            }
            return null;
        }

        public static bool CheckXml()
        {
            if (!System.IO.File.Exists(ShortCut.currentDirectory + @"\setting.xml")) {
                return false;
            }
            XmlDocument doc = new XmlDocument();
            doc.Load(ShortCut.currentDirectory + @"\setting.xml");
            XmlNode root = doc.DocumentElement.SelectSingleNode("/Setting");
            try
            {
                currentApp = root.Attributes["Primary"].Value;
            }
            catch (Exception e)
            {
                System.IO.File.Delete(ShortCut.currentDirectory + @"\setting.xml");
                System.Windows.MessageBox.Show("1.Support Wechat(have fun)\n2.Optimize the performance of memory patch\n3.Proxy is unstable due to some reasons, I recommand using memory patch", "What's new", MessageBoxButton.OK);
                return false;
            }
            return true;
        }

        public static void CreateXml(SortedDictionary<string, SortedDictionary<string, string>> dict)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement rootNode = (XmlElement)doc.AppendChild(doc.CreateElement("Setting"));
            rootNode.SetAttribute("Primary", currentApp);

            foreach (var each in dict)
            {
                var app = each.Value;
                XmlElement child = (XmlElement)rootNode.AppendChild(doc.CreateElement("App"));
                child.SetAttribute("Name", app["Name"]);
                child.SetAttribute("Path", app["Path"]);
                child.SetAttribute("Port", app["Port"]);
                child.SetAttribute("Mode", app["Mode"]);
                child.SetAttribute("Descript", app["Descript"]);
            }
            doc.Save(ShortCut.currentDirectory + @"\setting.xml");
        }

        public static void ModifyXml(string app, SortedDictionary<string,string> dict)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ShortCut.currentDirectory + @"\setting.xml");
            doc.DocumentElement.SetAttribute("Primary", app);
            XmlNode root = doc.DocumentElement.SelectSingleNode("/Setting");
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Setting/App");
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["Name"].Value == app)
                {
                    node.Attributes["Path"].Value = dict["Path"];
                    node.Attributes["Port"].Value = dict["Port"];
                    node.Attributes["Mode"].Value = dict["Mode"];
                    node.Attributes["Descript"].Value = dict["Descript"];
                    break;
                }
            }
            doc.Save(ShortCut.currentDirectory + @"\setting.xml");
        }

        public static string QueryXml(string app, string attr)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ShortCut.currentDirectory + @"\setting.xml");
            XmlNodeList nodes = doc.DocumentElement.SelectNodes("/Setting/App");
            foreach (XmlNode node in nodes)
            {
                if (node.Attributes["Name"].Value == app)
                {
                    return node.Attributes[attr].Value;
                }
            }
            return null;
        }
    }
}
