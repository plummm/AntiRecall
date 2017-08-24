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
using System.Globalization;
using System.Text;
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Class
         | AttributeTargets.Method)]
    public sealed class ExtensionAttribute : Attribute { }
}
namespace socks5
{
    public static class Extensions
    {
        public static string ToHex(this int value)
        {
            return String.Format("0x{0:X}", value);
        }
        public static int Find(this byte[] src, byte[] find, int startIndex = 0)
        {
            int index = -1;
            int matchIndex = 0;
            // handle the complete source array
            for (int i = startIndex; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }

        public static int FromHex(this string value)
        {
            // strip the leading 0x
            if (value.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            {
                value = value.Substring(2);
            }
            return Int32.Parse(value, NumberStyles.HexNumber);
        }

        public static int FindString(this byte[] src, string tofind, int startIndex = 0)
        {
            if (startIndex < 0) return -1;
            int index = -1;
            int matchIndex = 0;
            // handle the complete source array
            byte[] find = Encoding.ASCII.GetBytes(tofind);
            for (int i = startIndex; i < src.Length; i++)
            {
                if (src[i] == find[matchIndex])
                {
                    if (matchIndex == (find.Length - 1))
                    {
                        index = i - matchIndex;
                        break;
                    }
                    matchIndex++;
                }
                else
                {
                    matchIndex = 0;
                }

            }
            return index;
        }

        public static byte[] Replace(this byte[] src, byte[] search, byte[] repl)
        {
            byte[] dst = null;
            int index = src.Find(search);
            if (index >= 0)
            {
                dst = new byte[src.Length - search.Length + repl.Length];
                // before found array
                Buffer.BlockCopy(src, 0, dst, 0, index);
                // repl copy
                Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
                // rest of src array
                Buffer.BlockCopy(
                    src,
                    index + search.Length,
                    dst,
                    index + repl.Length,
                    src.Length - (index + search.Length));
                return dst;
            }
            return src;
        }

        public static byte[] ReplaceString(this byte[] src, string srch, string replace)
        {
            byte[] search = Encoding.ASCII.GetBytes(srch);
            byte[] repl = Encoding.ASCII.GetBytes(replace);
            byte[] dst = null;
            int index = src.Find(search);
            if (index >= 0)
            {
                dst = new byte[src.Length - search.Length + repl.Length];
                // before found array
                Buffer.BlockCopy(src, 0, dst, 0, index);
                // repl copy
                Buffer.BlockCopy(repl, 0, dst, index, repl.Length);
                // rest of src array
                Buffer.BlockCopy(
                    src,
                    index + search.Length,
                    dst,
                    index + repl.Length,
                    src.Length - (index + search.Length));
                return dst;
            }
            return src;
        }

        public static string GetBetween(this byte[] src, int start, int end)
        {
            byte[] dst = null;
            dst = new byte[end - start];
            Buffer.BlockCopy(src, start, dst, 0, (end - start));
            return Encoding.ASCII.GetString(dst);
        }

        public static byte[] GetInBetween(this byte[] src, int start, int end)
        {
            byte[] dst = null;
            dst = new byte[end - start];
            Buffer.BlockCopy(src, start, dst, 0, (end - start));
            return dst;
        }

        public static byte[] ReplaceBetween(this byte[] src, string start, string end, string replacement)
        {
            byte[] dst = null;
            //locate both.
            int index = src.FindString(start);
            int index1 = src.FindString(end, index);
            if(index > -1 && index1 > -1)
            {
                dst = new byte[src.Length - (index - index1) + replacement.Length];
                // before found array
                Buffer.BlockCopy(src, 0, dst, 0, index);
                // repl copy
                Buffer.BlockCopy(Encoding.ASCII.GetBytes(replacement), 0, dst, index, replacement.Length);
                // rest of src array
                Buffer.BlockCopy(
                    src,
                    index + (index1 - index),
                    dst,
                    index + replacement.Length,
                    src.Length - (index + (index1 - index)));
            }
            return dst;
        }

        public static byte[] ReplaceBetween(this byte[] src, int start, int end, byte[] replacement)
        {
            byte[] dst = null;
            dst = new byte[src.Length - (end - start) + replacement.Length];
            // before found array
            Buffer.BlockCopy(src, 0, dst, 0, start);
            // repl copy
            Buffer.BlockCopy(replacement, 0, dst, start, replacement.Length);
            // rest of src array
            Buffer.BlockCopy(
                src,
                start + (end - start),
                dst,
                start + replacement.Length,
                src.Length - (start + (end - start)));
            return dst;
        }

        public static T Pop<T>(this List<T> list)
        {
            T f = list[0];
            list.Remove(f);
            return f;
        }
        public static void Push<T>(this List<T> list, T item)
        {
            list.Add(item);
        }
    }
}
