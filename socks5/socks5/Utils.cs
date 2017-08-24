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

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;

namespace socks5
{
    public class Utils
    {
        private static Random r = new Random(Environment.TickCount);
        public static string RandStr(int count)
        {
            string abc = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string ret = "";
            for (int i = 0; i < count; i++)
            {
                ret += abc[r.Next(0, abc.Length - 1)];
            }
            return ret;
        }
        public static IPAddress GetInterfaceIPAddress(string IFName)
        {
            NetworkInterface[] netif = NetworkInterface.GetAllNetworkInterfaces();
            for (int i = 0; i < netif.Length; i++)
            {
                if (netif[i].Name == IFName)
                {
                    if (netif[i].GetIPProperties().UnicastAddresses.Count > 0)
                        return netif[i].GetIPProperties().UnicastAddresses[0].Address;
                    else return IPAddress.Any;
                }
            }
            return IPAddress.Any;
        }
    }
}
