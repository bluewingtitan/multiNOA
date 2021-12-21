using System;
using System.Collections;
using System.Linq;

namespace MultiNoa.Extensions
{
    internal static class ByteExtensions
    {
        internal static byte[] GetSubarray(this byte[] array, int offset, int length)
        {
            byte[] result = new byte[length];
            Array.Copy(array, offset, result, 0, length);
            return result;
        }

        internal static BitArray ToBitArray(this byte b)
        {
            return new BitArray(new []{b});
        }

        internal static byte? ToByte(this BitArray bits)
        {
            if (bits.Count != 8)
            {
                return null;
            }
            var bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            bytes = bytes.Reverse().ToArray();
            return bytes[0];
        }
    }
}