using IWshRuntimeLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiRecall.deploy
{
    class ShortCut
    {
        public static string currentDirectory { set; get; }
        public static bool is_lnk { set; get; }

        public static void init_shortcut(string filename)
        {
            currentDirectory = System.IO.Directory.GetCurrentDirectory();
            if (-1!=System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName.IndexOf(".lnk"))
            {
                is_lnk = true;
            }

            if (!CheckShortCut(filename))
            {
                CreateShortCut(filename);
            }
        }

        public static string GetLinkPath(string linkPath)
        {
            WshShell shell = new WshShell();
            IWshShortcut link = (IWshShortcut)shell.CreateShortcut(linkPath);
            return link.TargetPath;
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
