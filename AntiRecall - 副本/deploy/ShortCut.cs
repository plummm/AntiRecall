using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AntiRecall.deploy
{
    class ShortCut
    {

        public static string currentDirectory { set; get; }

        public static string myVersion { set; get; }


        /*
        public static void init_xml()
        {
            antiRElement = new SortedDictionary<string, string>();
            MainWindow window = (MainWindow)System.Windows.Application.Current.MainWindow;
            antiRElement.Add("PortText", window.PortText.Text);
            antiRElement.Add("QQPath", window.QQPath.Text);
            antiRElement.Add("is_hide_startup_notify", "0");
        }

        public static bool CheckXml()
        {
            return System.IO.File.Exists(currentDirectory + @"\setting.xml");
        }

        public static void CreateXml(SortedDictionary<string, string> dict)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement el = (XmlElement)doc.AppendChild(doc.CreateElement("Setting"));
            el.SetAttribute("QQPath", dict["QQPath"]);
            el.SetAttribute("PortText", dict["PortText"]);
            el.SetAttribute("is_hide_startup_notify", dict["is_hide_startup_notify"]);
            doc.Save(currentDirectory + @"\setting.xml");
        }

        public static void ModifyXml(SortedDictionary<string, string> dict)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(currentDirectory + @"\setting.xml");
            doc.DocumentElement.SetAttribute("QQPath", dict["QQPath"]);
            doc.DocumentElement.SetAttribute("PortText", dict["PortText"]);
            doc.DocumentElement.SetAttribute("is_hide_startup_notify", dict["is_hide_startup_notify"]);
            doc.Save(currentDirectory + @"\setting.xml");
            
        }
        
        public static string QueryXml(string attr)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(currentDirectory + @"\setting.xml");
            string ret = doc.DocumentElement.GetAttribute(attr);
            return ret;
        }
        */

        public static void init_shortcut(string filename)
        {
            if (currentDirectory == null)
                currentDirectory = System.IO.Directory.GetCurrentDirectory();
            myVersion = "1.2.0";

            if (!CheckShortCut(filename))
            {
                CreateShortCut(filename);
            }
        }

        private static bool CheckShortCut(string filename)
        {
            string desktopPath = string.Empty;
            desktopPath =
              string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                "\\", filename, ".lnk");
            return System.IO.File.Exists(desktopPath);
        }

        private static void CreateShortCut(string filename)
        {
            object shDesktop = (object)"Desktop";
            WshShell shell = new WshShell();
            string shortcutAddress = (string)shell.SpecialFolders.Item(ref shDesktop) + @"\" + filename + @".lnk";
            IWshShortcut shortcut = (IWshShortcut)shell.CreateShortcut(shortcutAddress);
            shortcut.Description = "New shortcut for a "+filename;
            shortcut.Hotkey = "Ctrl+Shift+Q";
            shortcut.TargetPath = currentDirectory + @"\" + filename + @".exe";
            shortcut.Save();
        }

       
    }
}
