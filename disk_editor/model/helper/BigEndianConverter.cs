using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace disk_editor
{
    class BigEndianConverter
    {
        public static byte[] GetBytes(bool value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(char value)
        {
            return BitConverter.GetBytes(value);
        }

        public static byte[] GetBytes(float value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 4);
        }

        public static byte[] GetBytes(double value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 8);
        }

        public static byte[] GetBytes(short value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 2);
        }

        public static byte[] GetBytes(int value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 4);
        }

        public static byte[] GetBytes(long value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 8);
        }

        public static byte[] GetBytes(ushort value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 2);
        }

        public static byte[] GetBytes(uint value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 4);
        }

        public static byte[] GetBytes(ulong value)
        {
            return BigEndianConverter.ReorderBytes(BitConverter.GetBytes(value), 0, 8);
        }

        public static float ToSingle(byte[] value, int startIndex)
        {
            return BitConverter.ToSingle(BigEndianConverter.ReorderBytes(value, startIndex, 4), 0);
        }

        public static double ToDouble(byte[] value, int startIndex)
        {
            return BitConverter.ToDouble(BigEndianConverter.ReorderBytes(value, startIndex, 8), 0);
        }

        public static short ToInt16(byte[] value, int startIndex)
        {
            return BitConverter.ToInt16(BigEndianConverter.ReorderBytes(value, startIndex, 2), 0);
        }

        public static int ToInt32(byte[] value, int startIndex)
        {
            return BitConverter.ToInt32(BigEndianConverter.ReorderBytes(value, startIndex, 4), 0);
        }

        public static long ToInt64(byte[] value, int startIndex)
        {
            return BitConverter.ToInt64(BigEndianConverter.ReorderBytes(value, startIndex, 8), 0);
        }

        public static ushort ToUInt16(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt16(BigEndianConverter.ReorderBytes(value, startIndex, 2), 0);
        }

        public static uint ToUInt32(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt32(BigEndianConverter.ReorderBytes(value, startIndex, 4), 0);
        }

        public static ulong ToUInt64(byte[] value, int startIndex)
        {
            return BitConverter.ToUInt64(BigEndianConverter.ReorderBytes(value, startIndex, 8), 0);
        }

        public static byte[] ReorderBytes(byte[] value, int startIndex, int count)
        {
            byte[] bytes = new byte[count];

            for (int i = (count - 1); i >= 0; i--)
            {
                var index = startIndex + (count - 1 - i);

                if (value.Length > index)
                {
                    bytes[i] = value[startIndex + (count - 1 - i)];
                }
                else
                {
                    bytes[i] = 0;
                }
            }

            return bytes;
        }
    }
}
