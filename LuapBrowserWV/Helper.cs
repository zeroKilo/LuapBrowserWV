using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LuapBrowserWV
{
    public static class Helper
    {
        public static bool isBigEndian = true;

        public static double ReadDouble(Stream s)
        {
            byte[] buff = new byte[8];
            s.Read(buff, 0, 8);
            if (isBigEndian)
            {
                ArraySwap(buff, 0, 7);
                ArraySwap(buff, 1, 6);
                ArraySwap(buff, 2, 5);
                ArraySwap(buff, 3, 4);
            }
            return BitConverter.ToDouble(buff, 0);
        }

        public static uint ReadU32(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            if (isBigEndian)
            {
                ArraySwap(buff, 0, 3);
                ArraySwap(buff, 1, 2);
            }
            return BitConverter.ToUInt32(buff, 0);
        }

        public static int ReadS32(Stream s)
        {
            byte[] buff = new byte[4];
            s.Read(buff, 0, 4);
            if (isBigEndian)
            {
                ArraySwap(buff, 0, 3);
                ArraySwap(buff, 1, 2);
            }
            return BitConverter.ToInt32(buff, 0);
        }

        public static string ReadString(Stream s)
        {
            uint count = Helper.ReadU32(s);
            if (count > 0)
            {
                string result = "";
                for (int i = 0; i < count - 1; i++)
                    result += (char)s.ReadByte();
                s.ReadByte();
                return result;
            }
            else
                return null;
        }

        public static void WriteDouble(Stream s, double v)
        {
            byte[] buff = BitConverter.GetBytes(v);
            if (isBigEndian)
            {
                ArraySwap(buff, 0, 7);
                ArraySwap(buff, 1, 6);
                ArraySwap(buff, 2, 5);
                ArraySwap(buff, 3, 4);
            }
            s.Write(buff, 0, 8);
        }

        public static void WriteU32(Stream s, uint v)
        {
            byte[] buff = BitConverter.GetBytes(v);
            if (isBigEndian)
            {
                ArraySwap(buff, 0, 3);
                ArraySwap(buff, 1, 2);
            }
            s.Write(buff, 0, 4);
        }

        public static void WriteS32(Stream s, int v)
        {
            byte[] buff = BitConverter.GetBytes(v);
            if (isBigEndian)
            {
                ArraySwap(buff, 0, 3);
                ArraySwap(buff, 1, 2);
            }
            s.Write(buff, 0, 4);
        }

        public static void WriteString(Stream s, string v)
        {
            if (v == null)
                WriteU32(s, 0);
            else
            {
                WriteS32(s, v.Length + 1);
                foreach (char c in v)
                    s.WriteByte((byte)c);
                s.WriteByte(0);
            }
        }

        public static void ArraySwap(byte[] arr, int idx1, int idx2)
        {
            byte tmp = arr[idx1];
            arr[idx1] = arr[idx2];
            arr[idx2] = tmp;
        }
    }
}
