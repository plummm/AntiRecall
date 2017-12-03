using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace AntiRecall.deploy
{
    class Xml
    {
        public static string QQ_ori_path;
        public static SortedDictionary<string, string> antiRElement;

        public static void init_xml()
        {
            antiRElement = new SortedDictionary<string, string>();
            MainWindow window = (MainWindow)System.Windows.Application.Current.MainWindow;
            antiRElement.Add("PortText", "");
            antiRElement.Add("QQPath", "");
            antiRElement.Add("Mode", "");
        }

        public static bool CheckXml()
        {
            return System.IO.File.Exists(ShortCut.currentDirectory + @"\setting.xml");
        }

        public static void CreateXml(SortedDictionary<string, string> dict)
        {
            XmlDocument doc = new XmlDocument();

            XmlElement el = (XmlElement)doc.AppendChild(doc.CreateElement("Setting"));
            el.SetAttribute("QQPath", dict["QQPath"]);
            el.SetAttribute("PortText", dict["PortText"]);
            el.SetAttribute("Mode", dict["Mode"]);
            doc.Save(ShortCut.currentDirectory + @"\setting.xml");
        }

        public static void ModifyXml(SortedDictionary<string, string> dict)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ShortCut.currentDirectory + @"\setting.xml");
            doc.DocumentElement.SetAttribute("QQPath", dict["QQPath"]);
            doc.DocumentElement.SetAttribute("PortText", dict["PortText"]);
            doc.DocumentElement.SetAttribute("Mode", dict["Mode"]);
            doc.Save(ShortCut.currentDirectory + @"\setting.xml");
            /*XmlNodeList nodelist = doc.GetElementsByTagName("Setting");
            foreach (XmlNode set in nodelist)
            {
                ((XmlElement)set).SetAttribute(attr, path);
            }*/

        }

        public static string QueryXml(string attr)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(ShortCut.currentDirectory + @"\setting.xml");
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
    }
}
