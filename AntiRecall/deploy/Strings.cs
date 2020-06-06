using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntiRecall.deploy
{
    class Strings
    {
        public static string explorer_ready = "Ready to Go";
        public static string explorer_hold = "Install Path";
        public static string invalid_method = "Choose a valid anti-recall method";
        public static string incorrect_target_path = "Fail to boot target application，Please check if it's a correct path";
        public static string invalid_target_path = "Cannot detect any executeable application. Please check if it's a correct path";
        public static string title = "AntiRecall";
        public static string loaded_module = "Antirecall mudule has been loaded.";
        public static string failed_loaded_module = "Fail to load antirecall module, close antivirus and try again";
        public static string minimized = "Minimized AntiRecall, running on the background";
        public static string launch_stopped = "Stopped";
        public static string launch_started = "Started";
        public static string proxy_warning = "Proxy is not recommended any more. Do you really want to use proxy?";
        public static string warning = "Warning";
    }
}
