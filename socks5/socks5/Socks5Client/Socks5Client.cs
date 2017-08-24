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
using System.Text;
using socks5.TCP;
using System.Net.Sockets;
using socks5.Socks;
using socks5.Encryption;

namespace socks5.Socks5Client
{
    public class Socks5Client
    {
        private IPAddress ipAddress;
        public Client Client;

        private Socket p;
        private int Port;
        public bool reqPass = false;

        private byte[] HalfReceiveBuffer = new byte[4200];
        private int HalfReceivedBufferLength = 0;

        private string Username;
        private string Password;
        private string Dest;
        private int Destport;

        public Encryption.SocksEncryption enc;

        public IList<AuthTypes> UseAuthTypes { get; set; }

        public event EventHandler<Socks5ClientArgs> OnConnected = delegate { };
        public event EventHandler<Socks5ClientDataArgs> OnDataReceived = delegate { };
        public event EventHandler<Socks5ClientDataArgs> OnDataSent = delegate { };
        public event EventHandler<Socks5ClientArgs> OnDisconnected = delegate { };

        private Socks5Client()
        {
            UseAuthTypes = new List<AuthTypes>(new[] { AuthTypes.None, AuthTypes.Login, AuthTypes.SocksEncrypt });
        }

        public Socks5Client(string ipOrDomain, int port, string dest, int destport, string username = null, string password = null)
            : this()
        {
            //Parse IP?
            if (!IPAddress.TryParse(ipOrDomain, out ipAddress))
            {
                //not connected.
                try
                {
                    foreach (IPAddress p in Dns.GetHostAddresses(ipOrDomain))
                        if (p.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                        {
                            DoSocks(p, port, dest, destport, username, password);
                            return;
                        }
                }
                catch
                {
                    throw new NullReferenceException();
                }
            }           
            DoSocks(ipAddress, port, dest, destport, username, password);
        }
        public Socks5Client(IPAddress ip, int port, string dest, int destport, string username = null, string password = null)
            : this()
        {
            DoSocks(ip, port, dest, destport, username, password);
        }

        private void DoSocks(IPAddress ip, int port, string dest, int destport, string username = null, string password = null)
        {
            ipAddress = ip;
            Port = port;
            //check for username & pw.
            if(username != null && password != null)
            {
                Username = username;
                Password = password;
                reqPass = true;
            }
            Dest = dest;
            Destport = destport;
        }

        public void ConnectAsync()
        {
            //
            p = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            Client = new Client(p, 4200);
            Client.onClientDisconnected += Client_onClientDisconnected;
            Client.Sock.BeginConnect(new IPEndPoint(ipAddress, Port), new AsyncCallback(onConnected), Client);
            //return status?
        }

        void Client_onClientDisconnected(object sender, ClientEventArgs e)
        {
            this.OnDisconnected(this, new Socks5ClientArgs(this, SocksError.Expired));
        }

        public bool Send(byte[] buffer, int offset, int length)
        {
            try
            {
                //buffer sending.
                int offst = 0;
                while(true)
                {
                    byte[] outputdata = enc.ProcessOutputData(buffer, offst, (length - offst > 4092 ? 4092 : length - offst));
                    offst += (length - offst > 4092 ? 4092 : length - offst);
                    //craft headers & shit.
                    //send outputdata's length firs.t
                    if (enc.GetAuthType() != AuthTypes.Login && enc.GetAuthType() != AuthTypes.None)
                    {
                        byte[] datatosend = new byte[outputdata.Length + 4];
                        Buffer.BlockCopy(outputdata, 0, datatosend, 4, outputdata.Length);
                        Buffer.BlockCopy(BitConverter.GetBytes(outputdata.Length), 0, datatosend, 0, 4);
                        outputdata = null;
                        outputdata = datatosend;
                    }
                    Client.Send(outputdata, 0, outputdata.Length);
                    if (offst >= buffer.Length)
                    {
                        //exit;
                        return true;
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public bool Send(byte[] buffer)
        {
            return Send(buffer, 0, buffer.Length);
        }

        public int Receive(byte[] buffer, int offset, int count)
        {
            //this should be packet header.
            try
            {
                if (enc.GetAuthType() != AuthTypes.Login && enc.GetAuthType() != AuthTypes.None)
                {
                    if(HalfReceivedBufferLength > 0)
                    {
                        if (HalfReceivedBufferLength <= count)
                        {
                            Buffer.BlockCopy(HalfReceiveBuffer, 0, buffer, offset, HalfReceivedBufferLength);
                            HalfReceivedBufferLength = 0;
                            return HalfReceivedBufferLength;
                        }
                        else
                        {
                            Buffer.BlockCopy(HalfReceiveBuffer, 0, buffer, offset, count);
                            HalfReceivedBufferLength = HalfReceivedBufferLength - count;
                            Buffer.BlockCopy(HalfReceiveBuffer, count, HalfReceiveBuffer, 0, count);

                            return count;
                        }
                    }

                    count = Math.Min(4200, count);

                    byte[] databuf = new byte[4200];
                    int got = Client.Receive(databuf, 0, 4200);

                    int packetsize = BitConverter.ToInt32(databuf, 0);
                    byte[] processed = enc.ProcessInputData(databuf, 4, packetsize);

                    Buffer.BlockCopy(databuf, 0, buffer, offset, count);
                    Buffer.BlockCopy(databuf, count, HalfReceiveBuffer, 0, packetsize - count);
                    HalfReceivedBufferLength = packetsize - count;
                    return count;
                }
                else
                {
                    return Client.Receive(buffer, offset, count);
                }
            }
            catch (Exception ex)
            {
                //disconnect.
                Client.Disconnect();
                throw ex;
            }
        }

        public void ReceiveAsync()
        {
            if (enc.GetAuthType() != AuthTypes.Login && enc.GetAuthType() != AuthTypes.None)
            {
                Client.ReceiveAsync(4);
            }
            else
            {
                Client.ReceiveAsync(4096);
            }
        }


        void Client_onDataReceived(object sender, DataEventArgs e)
        {
            //this should be packet header.
            try
            {
                if (enc.GetAuthType() != AuthTypes.Login && enc.GetAuthType() != AuthTypes.None)
                {
                    //get total number of bytes.
                    int torecv = BitConverter.ToInt32(e.Buffer, 0);
                    byte[] newbuff = new byte[torecv];

                    int recvd = Client.Receive(newbuff, 0, torecv);
                    if (recvd == torecv)
                    {
                        byte[] output = enc.ProcessInputData(newbuff, 0, recvd);
                        //receive full packet.
                        e.Buffer = output;
                        e.Offset = 0;
                        e.Count = output.Length;
                        this.OnDataReceived(this, new Socks5ClientDataArgs(this, e.Buffer, e.Count, e.Offset));
                    }
                }
                else
                {
                    this.OnDataReceived(this, new Socks5ClientDataArgs(this, e.Buffer, e.Count, e.Offset));
                }
            }
            catch (Exception ex)
            {
                //disconnect.
                Client.Disconnect();
                throw ex;
            }
        }

        public bool Connect()
        {
            try
            {
                p = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                Client = new Client(p, 65535);
                Client.Sock.Connect(new IPEndPoint(ipAddress, Port));
                //try the greeting.
                //Client.onDataReceived += Client_onDataReceived;
                if(Socks.DoSocksAuth(this, Username, Password))
                    if (Socks.SendRequest(Client, enc, Dest, Destport) == SocksError.Granted) {
                        Client.onDataReceived += Client_onDataReceived;
                        return true;
                    }
                return false;
            }
            catch
            {
                return false;
            }
        }

        private void onConnected(IAsyncResult res)
        {
            Client = (Client)res.AsyncState;
            try
            {
                Client.Sock.EndConnect(res);
            }
            catch
            {
                this.OnConnected(this, new Socks5ClientArgs(null, SocksError.Failure));
                return;
            }
            if (Socks.DoSocksAuth(this, Username, Password))
            {
                SocksError p = Socks.SendRequest(Client, enc, Dest, Destport);
                Client.onDataReceived += Client_onDataReceived;
                this.OnConnected(this, new Socks5ClientArgs(this, p));
                
            }
            else
                this.OnConnected(this, new Socks5ClientArgs(this, SocksError.Failure));
        }


        public bool Connected
        {
            get { return (Client != null ? Client.Sock.Connected : false); }
        }
        //send.

    }
}
