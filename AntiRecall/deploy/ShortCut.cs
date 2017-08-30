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

        public static SortedDictionary<string, string> antiRElement;

        public static void init_xml()
        {
            antiRElement = new SortedDictionary<string, string>();
            MainWindow window = (MainWindow)System.Windows.Application.Current.MainWindow;
            antiRElement.Add("PortText", window.PortText.Text);
            antiRElement.Add("QQPath", window.QQPath.Text);
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
            doc.Save(currentDirectory + @"\setting.xml");
        }

        public static void ModifyXml(SortedDictionary<string, string> dict)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(currentDirectory + @"\setting.xml");
            doc.DocumentElement.SetAttribute("QQPath", dict["QQPath"]);
            doc.DocumentElement.SetAttribute("PortText", dict["PortText"]);
            doc.Save(currentDirectory + @"\setting.xml");
            /*XmlNodeList nodelist = doc.GetElementsByTagName("Setting");
            foreach (XmlNode set in nodelist)
            {
                ((XmlElement)set).SetAttribute(attr, path);
            }*/
            
        }

        public static string QueryXml(string attr)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(currentDirectory + @"\setting.xml");
            string ret = doc.DocumentElement.GetAttribute(attr);
            return ret;
            /*
            XmlNodeList nodelist = doc.GetElementsByTagName("Setting");
            foreach (XmlNode set in nodelist)
            {
                string ret = ((XmlElement)set).GetAttribute(attr);
                return ret;
            }
            return null;
            */
        }

        /*
        public static string GetLinkPath(string linkPath)
        {
            WshShell shell = new WshShell();
            IWshShortcut link = (IWshShortcut)shell.CreateShortcut(linkPath);
            return link.TargetPath;
        }
        */

        public static void init_shortcut(string filename)
        {
            if (currentDirectory == null)
                currentDirectory = System.IO.Directory.GetCurrentDirectory();


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
