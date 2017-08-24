using System;
using System.Collections.Generic;
using System.Text;
using socks5.Plugin;
namespace socks5.Examples
{
    class LoadCustomPlugin
    {
        public LoadCustomPlugin()
        {
            PluginLoader.LoadCustomPlugin(typeof(MyCustomPlugin));
        }
    }
    public class MyCustomPlugin : LoginHandler
    {
        public override Plugin.LoginStatus HandleLogin(TCP.User user)
        {
            return (user.Username == "test" && user.Password == "testing1234" && user.IP.ToString().StartsWith("192.168.1.") ? LoginStatus.Correct : LoginStatus.Denied);
        }

        private bool enabled = true;
        public override bool Enabled
        {
            get
            {
                return enabled;
            }
            set
            {
                enabled = value;
            }
        }
    }
}
