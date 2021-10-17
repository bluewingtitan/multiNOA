using System;

namespace MultiNOA.Networking.Common.NetworkData.DataContainer
{
    public class NetworkLong : INetworkDataContainer<long>
    {
        #region Operators
        public static NetworkLong operator +(NetworkLong nb, short n) => new NetworkLong((nb._v + n));
        public static NetworkLong operator +(short n, NetworkLong nb) => new NetworkLong((nb._v + n));
        public static NetworkLong operator -(NetworkLong nb, short n) => new NetworkLong((nb._v - n));
        public static NetworkLong operator -(short n, NetworkLong nb) => new NetworkLong((n - nb._v));
        public static NetworkLong operator + (NetworkLong nb) => new NetworkLong(nb._v);
        public static NetworkLong operator ++(NetworkLong nb)
        {
            nb._v++;
            return nb;
        }
        public static NetworkLong operator --(NetworkLong nb)
        {
            nb._v--;
            return nb;
        }

        #endregion

        private const int LongByteLength = 8;
        
        private long _v;

        public NetworkLong(long value = 0)
        {
            _v = value;
        }
        
        public byte[] TurnIntoBytes()
        {
            return BitConverter.GetBytes(_v);
        }

        public long GetValue()
        {
            return _v;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = BitConverter.ToInt16(bytes, 0); // Read int from bytes.
            return LongByteLength;
        }
    }
}