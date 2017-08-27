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
using socks5.Socks;
using socks5.TCP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using System.Text;
using System.Drawing;

namespace socks5
{
    class SocksTunnel
    {
        public SocksRequest Req;
        public SocksRequest ModifiedReq;

        public SocksClient Client;
        public Client RemoteClient;

        private List<DataHandler> Plugins = new List<DataHandler>();

        private int Timeout = 10000;
        private int PacketSize = 4096;

        private static bool is_recallmodule_load;
        private NotifyIcon module_ni;

        private void disposeBallon(MouseEventHandler value)
        {
            module_ni.Visible = false;
        }

        public SocksTunnel(SocksClient p, SocksRequest req, SocksRequest req1, int packetSize, int timeout)
        {
            RemoteClient = new Client(new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp), PacketSize);
            Client = p;
            Req = req;
            ModifiedReq = req1;
            PacketSize = packetSize;
            Timeout = timeout;
        }

        SocketAsyncEventArgs socketArgs;

        public void Open(IPAddress outbound)
        {
            if (ModifiedReq.Address == null || ModifiedReq.Port <= -1) { Client.Client.Disconnect(); return; }

            if (!is_recallmodule_load)
            {
                if (ModifiedReq.Address.Equals("httpring.qq.com"))
                {
                    is_recallmodule_load = true;

                    module_ni = new NotifyIcon();
                    module_ni.Icon = SystemIcons.Exclamation;
                    module_ni.Visible = true;
                    module_ni.BalloonTipTitle = "AntiRecall";
                    module_ni.BalloonTipText = "防撤回模块已成功加载";
                    module_ni.BalloonTipIcon = ToolTipIcon.Info;
                    module_ni.ShowBalloonTip(30000);
                    module_ni.Visible = false;
                    module_ni.Dispose();
                }
            }
#if DEBUG
            Console.WriteLine("{0}({1}):{2}", ModifiedReq.Address, ModifiedReq.IP, ModifiedReq.Port);
#endif
            foreach (ConnectSocketOverrideHandler conn in PluginLoader.LoadPlugin(typeof(ConnectSocketOverrideHandler)))
            if(conn.Enabled)
            {
                Client pm = conn.OnConnectOverride(ModifiedReq);
                if (pm != null)
                {
                    //check if it's connected.
                    if (pm.Sock.Connected)
                    {
                        RemoteClient = pm;
                        //send request right here.
                        byte[] shit = Req.GetData(true);
                        shit[1] = 0x00;
                        //gucci let's go.
                        Client.Client.Send(shit);
                        ConnectHandler(null);
                        return;
                    }
                }
            }
            if (ModifiedReq.Error != SocksError.Granted)
            {
                Client.Client.Send(Req.GetData(true));
                Client.Client.Disconnect();
                return;
            }
            socketArgs = new SocketAsyncEventArgs { RemoteEndPoint = new IPEndPoint(ModifiedReq.IP, ModifiedReq.Port) };
            socketArgs.Completed += socketArgs_Completed;
            RemoteClient.Sock = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            RemoteClient.Sock.Bind(new IPEndPoint(outbound, 0));
            if (!RemoteClient.Sock.ConnectAsync(socketArgs))
                ConnectHandler(socketArgs);
        }

        void socketArgs_Completed(object sender, SocketAsyncEventArgs e)
        {
            
            byte[] request = Req.GetData(true); // Client.Client.Send(Req.GetData());
            if (e.SocketError != SocketError.Success)
            {
                Console.WriteLine("Error while connecting: {0}", e.SocketError.ToString());
                request[1] = (byte)SocksError.Unreachable;
            }
            else
            {
                request[1] = 0x00;
            }

            Client.Client.Send(request);

            if(socketArgs != null)
            {
                socketArgs.Completed -= socketArgs_Completed;
                socketArgs.Dispose();
            }

            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    //connected;
                    ConnectHandler(e);
                    break;               
            }
        }

        private void ConnectHandler(SocketAsyncEventArgs e)
        {
            //start receiving from both endpoints.
            try
            {
                //all plugins get the event thrown.
                foreach (DataHandler data in PluginLoader.LoadPlugin(typeof(DataHandler)))
                    Plugins.Push(data);
                Client.Client.onDataReceived += Client_onDataReceived;
                RemoteClient.onDataReceived += RemoteClient_onDataReceived;
                RemoteClient.onClientDisconnected += RemoteClient_onClientDisconnected;
                Client.Client.onClientDisconnected += Client_onClientDisconnected;
                RemoteClient.ReceiveAsync();
                Client.Client.ReceiveAsync();
            }
            catch
            {
                RemoteClient.Disconnect();
                Client.Client.Disconnect();
            }
        }
        bool disconnected = false;
        void Client_onClientDisconnected(object sender, ClientEventArgs e)
        {
            if (disconnected) return;
            //Console.WriteLine("Client DC'd");
            disconnected = true;
            RemoteClient.Disconnect();
            RemoteClient.onDataReceived -= RemoteClient_onDataReceived;
            RemoteClient.onClientDisconnected -= RemoteClient_onClientDisconnected;
        }

        void RemoteClient_onClientDisconnected(object sender, ClientEventArgs e)
        {
            
#if DEBUG
            Console.WriteLine("Remote DC'd");
#endif
            if (disconnected) return;
            //Console.WriteLine("Remote DC'd");
            disconnected = true;
            Client.Client.Disconnect();
            Client.Client.onDataReceived -= Client_onDataReceived;
            Client.Client.onClientDisconnected -= Client_onClientDisconnected;
        }

        void RemoteClient_onDataReceived(object sender, DataEventArgs e)
        {
            e.Request = this.ModifiedReq;
            foreach (DataHandler f in Plugins)
                f.OnServerDataReceived(this, e);
            Client.Client.Send(e.Buffer, e.Offset, e.Count);
            if (!RemoteClient.Receiving)
                RemoteClient.ReceiveAsync();
            if (!Client.Client.Receiving)
                Client.Client.ReceiveAsync();
        }

        void Client_onDataReceived(object sender, DataEventArgs e)
        {
            e.Request = this.ModifiedReq;
            foreach (DataHandler f in Plugins)
                f.OnClientDataReceived(this, e);
            
            RemoteClient.Send(e.Buffer, e.Offset, e.Count);
            if (!Client.Client.Receiving)
                Client.Client.ReceiveAsync();
            if (!RemoteClient.Receiving)
                RemoteClient.ReceiveAsync();
        }
    }
}
