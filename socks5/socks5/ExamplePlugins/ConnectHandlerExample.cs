/*
    Socks5 - A full-fledged high-performance socks5 proxy server written in C#. Plugin support included.
    Copyright (C) 2016 ThrDev

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using socks5.Plugin;
using System;
using System.Collections.Generic;
using System.Text;

namespace socks5.ExamplePlugins
{
    public class ConnectHandlerExample : ConnectHandler
    {
        public override bool OnStart()
        {
            return true;
        }

        public override bool OnConnect(Socks.SocksRequest Request)
        {
            //Compare data.
            if (Request.Address.Contains("74.125.224")) //Google.com IP
            {
                Console.WriteLine("Redirecting traffic from {0} to yahoo.com.", Request.Address);
                Request.Address = "www.yahoo.com";
                Request.Type = Socks.AddressType.Domain;
            }
            //Allow the connection.
            return true;
        }
        private bool enabled = false;
        public override bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }
    }
}
