using System;
using System.Collections.Generic;
using System.Text;

namespace socks5.Examples
{
    class EnablePlugin
    {
		public EnablePlugin()
		{
			PluginLoader.ChangePluginStatus(true, typeof(MyCustomPlugin));
		}
    }
	public class MyCustomPlugin : LoginHandler
    {
        public override Plugin.LoginStatus HandleLogin(TCP.User user)
        {
            return (user.Username == "test" && user.Password == "testing1234" && user.IP.ToString().StartsWith("192.168.1.") ? LoginStatus.Correct : LoginStatus.Denied);
        }

        private bool enabled = false;
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
