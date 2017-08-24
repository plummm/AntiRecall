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
using System.Text;

namespace socks5
{
    public class BandwidthCounter
    {
        /// <summary>
        /// Class to manage an adapters current transfer rate
        /// </summary>
        class MiniCounter
        {
            public ulong bytes = 0;
            public ulong kbytes = 0;
            public ulong mbytes = 0;
            public ulong gbytes = 0;
            public ulong tbytes = 0;
            public ulong pbytes = 0;
            DateTime lastRead = DateTime.Now;

            /// <summary>
            /// Adds bits(total misnomer because bits per second looks a lot better than bytes per second)
            /// </summary>
            /// <param name="count">The number of bits to add</param>
            public void AddBytes(ulong count)
            {
                bytes += count;
                while (bytes > 1024)
                {
                    kbytes++;
                    bytes -= 1024;
                }
                while (kbytes > 1024)
                {
                    mbytes++;
                    kbytes -= 1024;
                }
                while (mbytes > 1024)
                {
                    gbytes++;
                    mbytes -= 1024;
                }
                while (gbytes > 1024)
                {
                    tbytes++;
                    gbytes -= 1024;
                }
                while (tbytes > 1024)
                {
                    pbytes++;
                    tbytes -= 1024;
                }
            }


            public ulong BytesPerSec()
            {
                if (gbytes > 0)
                {
                    double ret = (double)gbytes + ((double)((double)mbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    
                    return (ulong)(((ret * 1024) * 1024) * 1024);
                }
                else if (mbytes > 0)
                {
                    double ret = (double)mbytes + ((double)((double)kbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;

                    return (ulong)((ret * 1024) * 1024);
                }
                else if (kbytes > 0)
                {
                    double ret = (double)kbytes + ((double)((double)bytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;

                    return (ulong)(ret * 1024);
                }
                else
                {
                    double ret = bytes;
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;

                    return (ulong)ret;
                }
            }

            /// <summary>
            /// Returns the bits per second since the last time this function was called
            /// </summary>
            /// <returns></returns>
            public override string ToString()
            {
                if (pbytes > 0)
                {
                    double ret = (double)pbytes + ((double)((double)tbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " PB";
                }
                else if (tbytes > 0)
                {
                    double ret = (double)tbytes + ((double)((double)gbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " TB";
                }
                else if (gbytes > 0)
                {
                    double ret = (double)gbytes + ((double)((double)mbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " GB";
                }
                else if (mbytes > 0)
                {
                    double ret = (double)mbytes + ((double)((double)kbytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;

                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " MB";
                }
                else if (kbytes > 0)
                {
                    double ret = (double)kbytes + ((double)((double)bytes / 1024));
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " KB";
                }
                else
                {
                    double ret = bytes;
                    ret = ret / (DateTime.Now - lastRead).TotalSeconds;
                    lastRead = DateTime.Now;
                    string s = ret.ToString();
                    if (s.Length > 6)
                        s = s.Substring(0, 6);
                    return s + " B";
                }
            }
        }

        private ulong bytes = 0;
        private ulong kbytes = 0;
        private ulong mbytes = 0;
        private ulong gbytes = 0;
        private ulong tbytes = 0;
        private ulong pbytes = 0;
        MiniCounter perSecond = new MiniCounter();

        /// <summary>
        /// Empty constructor, because thats constructive
        /// </summary>
        public BandwidthCounter()
        {

        }

        /// <summary>
        /// Accesses the current transfer rate, returning the text
        /// </summary>
        /// <returns></returns>
        public string GetPerSecond()
        {
            string s = perSecond.ToString() + "/s";
            perSecond = new MiniCounter();
            return s;
        }

        public ulong GetPerSecondNumeric()
        {
            ulong val = perSecond.BytesPerSec();
            perSecond = new MiniCounter();
            return val;
        }

        /// <summary>
        /// Adds bytes to the total transfered
        /// </summary>
        /// <param name="count">Byte count</param>
        public void AddBytes(ulong count)
        {
            // overflow max
            perSecond.AddBytes(count);
            bytes += count;
            while (bytes > 1024)
            {
                kbytes++;
                bytes -= 1024;
            }
            while (kbytes > 1024)
            {
                mbytes++;
                kbytes -= 1024;
            }
            while (mbytes > 1024)
            {
                gbytes++;
                mbytes -= 1024;
            }
            while (gbytes > 1024)
            {
                tbytes++;
                gbytes -= 1024;
            }
            while (tbytes > 1024)
            {
                pbytes++;
                tbytes -= 1024;
            }
        }

        /// <summary>
        /// Prints out a relevant string for the bits transfered
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (pbytes > 0)
            {
                double ret = (double)pbytes + ((double)((double)tbytes / 1024));
                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " Pb";
            }
            else if (tbytes > 0)
            {
                double ret = (double)tbytes + ((double)((double)gbytes / 1024));

                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " TB";
            }
            else if (gbytes > 0)
            {
                double ret = (double)gbytes + ((double)((double)mbytes / 1024));
                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " GB";
            }
            else if (mbytes > 0)
            {
                double ret = (double)mbytes + ((double)((double)kbytes / 1024));

                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " MB";
            }
            else if (kbytes > 0)
            {
                double ret = (double)kbytes + ((double)((double)bytes / 1024));

                string s = ret.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " KB";
            }
            else
            {
                string s = bytes.ToString();
                if (s.Length > 6)
                    s = s.Substring(0, 6);
                return s + " b";
            }
        }
    }
}
