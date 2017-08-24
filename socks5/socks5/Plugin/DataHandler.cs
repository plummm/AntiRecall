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

using socks5.TCP;
using System;
using System.Collections.Generic;
using System.Text;

namespace socks5.Plugin
{
    public abstract class DataHandler : GenericPlugin
    {
        /// <summary>
        /// Allows you to grab/modify data before it's sent to the end user.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void OnServerDataReceived(object sender, DataEventArgs e);

        /// <summary>
        /// Allows you to grab/modify data before it's sent to the client.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public abstract void OnClientDataReceived(object sender, DataEventArgs e);

        public abstract bool OnStart();

        public abstract bool Enabled { get; set; }
    }
}
