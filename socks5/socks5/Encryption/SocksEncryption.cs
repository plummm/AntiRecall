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

using socks5.Socks;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace socks5.Encryption
{
    public class SocksEncryption
    {
        public SocksEncryption()
        {

        }

        public RSACryptoServiceProvider key;
        private RSACryptoServiceProvider remotepubkey;
        private DarthEncrypt dc;
        private DarthEncrypt dcc;
        private AuthTypes auth;

        public void GenerateKeys()
        {
            key = new RSACryptoServiceProvider(1024);
            remotepubkey = new RSACryptoServiceProvider(1024);
            remotepubkey.PersistKeyInCsp = false;
            key.PersistKeyInCsp = false;
            dc = new DarthEncrypt();
            dc.PassPhrase = Utils.RandStr(20);
            dcc = new DarthEncrypt();
        }

        public byte[] ShareEncryptionKey()
        {
            //share public key.
            return remotepubkey.Encrypt(Encoding.ASCII.GetBytes(dc.PassPhrase), false);
        }

        public byte[] GetPublicKey()
        {
            return Encoding.ASCII.GetBytes(key.ToXmlString(false));
        }

        public void SetEncKey(byte[] key)
        {
            dcc.PassPhrase = Encoding.ASCII.GetString(key);
        }

        public void SetKey(byte[] key, int offset, int len)
        {
            string e = Encoding.ASCII.GetString(key, offset, len);
            remotepubkey.FromXmlString(e);
        }

        public void SetType(AuthTypes k)
        {
            auth = k;
        }

        public AuthTypes GetAuthType()
        {
            return auth;
        }

        public byte[] ProcessInputData(byte[] buffer, int offset, int count)
        {
            //realign buffer.
            try
            {
                byte[] buff = new byte[count];
                Buffer.BlockCopy(buffer, offset, buff, 0, count);
                switch (this.auth)
                {
                    case AuthTypes.SocksBoth:
                        //decrypt, then decompress.
                        byte[] data = this.dcc.DecryptBytes(buff);
                        return dcc.DecompressBytes(data);
                    case AuthTypes.SocksCompress:
                        //compress data.
                        return dcc.DecompressBytes(buff);
                    case AuthTypes.SocksEncrypt:
                        return dcc.DecryptBytes(buff);
                    default:
                        return buffer;
                }
            }
            catch {
                return null;
            }
        }

        public byte[] ProcessOutputData(byte[] buffer, int offset, int count)
        {
            //realign buffer.
            try
            {
                byte[] buff = new byte[count - offset];
                Buffer.BlockCopy(buffer, offset, buff, 0, count);
                switch (this.auth)
                {
                    case AuthTypes.SocksBoth:
                        //compress, then encrypt.
                        byte[] data = dc.CompressBytes(buff, 0, count);
                        return this.dc.EncryptBytes(data);
                    case AuthTypes.SocksCompress:
                        //compress data.
                        return dc.CompressBytes(buff, 0, count);
                    case AuthTypes.SocksEncrypt:
                        return dc.EncryptBytes(buff);
                    default:
                        return buffer;
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
