using System;

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
    }
}