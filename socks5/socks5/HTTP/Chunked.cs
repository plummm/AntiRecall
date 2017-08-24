using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using socks5;
namespace socks5.HTTP
{
    //WARNING: BETA - Doesn't work as well as intended. Use at your own discretion.
    public class Chunked
    {
        private byte[] totalbuff;
        private byte[] finalbuff;
        /// <summary>
        /// Create a new instance of chunked.
        /// </summary>
        /// <param name="f"></param>
        public Chunked(Socket f, byte[] oldbuffer, int size)
        {
            //Find first chunk.
            if (IsChunked(oldbuffer))
            {
                int endofheader = oldbuffer.FindString("\r\n\r\n");
                int endofchunked = oldbuffer.FindString("\r\n", endofheader + 4);
                //
                string chunked = oldbuffer.GetBetween(endofheader + 4, endofchunked);
                //convert chunked data to int.
                int totallen = chunked.FromHex();
                //
                if (totallen > 0)
                {
                    //start a while loop and receive till end of chunk.
                    totalbuff = new byte[65535];
                    finalbuff = new byte[size];
                    //remove chunk data before adding.
                    oldbuffer = oldbuffer.ReplaceBetween(endofheader + 4, endofchunked + 2, new byte[] { });
                    Buffer.BlockCopy(oldbuffer, 0, finalbuff, 0, size);
                    if (f.Connected)
                    {
                        int totalchunksize = 0;
                        int received = f.Receive(totalbuff, 0, totalbuff.Length, SocketFlags.None);
                        while ((totalchunksize = GetChunkSize(totalbuff, received)) != -1)
                        {
                            //add data to final byte buffer.
                            byte[] chunkedData = GetChunkData(totalbuff, received);
                            byte[] tempData = new byte[chunkedData.Length + finalbuff.Length];
                            //get data AFTER chunked response.
                            Buffer.BlockCopy(finalbuff, 0, tempData, 0, finalbuff.Length);
                            Buffer.BlockCopy(chunkedData, 0, tempData, finalbuff.Length, chunkedData.Length);
                            //now add to finalbuff.
                            finalbuff = tempData;
                            //receive again.
                            if (totalchunksize == -2)
                                break;
                            else
                                received = f.Receive(totalbuff, 0, totalbuff.Length, SocketFlags.None);

                        }
                        //end of chunk.
                        Console.WriteLine("Got chunk! Size: {0}", finalbuff.Length);
                    }
                }
                else
                {
                    finalbuff = new byte[size];
                    Buffer.BlockCopy(oldbuffer, 0, finalbuff, 0, size);
                }
            }
        }

        public byte[] RawData
        {
            get
            {
                return finalbuff;
            }
        }

        public byte[] ChunkedData
        {
            get
            {
                //get size from \r\n\r\n and past.
                int location = finalbuff.FindString("\r\n\r\n") + 4;
                //size
                int size = finalbuff.Length - location - 7; //-7 is initial end of chunk data.
                return finalbuff.ReplaceString("\r\n\r\n", "\r\n\r\n" + size.ToHex().Replace("0x", "") + "\r\n");
            }
        }

        public static int GetChunkSize(byte[] buffer, int count)
        {
            //chunk size is first chars till \r\n.
            if(buffer.FindString("\r\n0\r\n\r\n", count - 7) != -1)
            {
                //end of buffer.
                return -2;
            }
            string chunksize = buffer.GetBetween(0, buffer.FindString("\r\n"));
            return chunksize.FromHex();
        }

        public static byte[] GetChunkData(byte[] buffer, int size)
        {
            //parse out the chunk size and return data.
            return buffer.GetInBetween(buffer.FindString("\r\n") + 2, size);
        }

        public static bool IsChunked(byte[] buffer)
        {
            return (IsHTTP(buffer) && buffer.FindString("Transfer-Encoding: chunked\r\n") != -1);
        }

        public static bool IsHTTP(byte[] buffer)
        {
            return (buffer.FindString("HTTP/1.1") != -1 && buffer.FindString("\r\n\r\n") != -1);
        }
    }
}
