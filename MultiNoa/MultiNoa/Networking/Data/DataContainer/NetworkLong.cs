using System;

namespace MultiNoa.Networking.Data.DataContainer
{
    [MultiNoa.Networking.PacketHandling.DataContainer(typeof(long))]
    public struct NetworkLong : INetworkDataContainer<long>
    {
        #region Operators
        public static NetworkLong operator +(NetworkLong nb, long n) => new NetworkLong((nb._v + n));
        public static NetworkLong operator +(long n, NetworkLong nb) => new NetworkLong((nb._v + n));
        public static NetworkLong operator -(NetworkLong nb, long n) => new NetworkLong((nb._v - n));
        public static NetworkLong operator -(long n, NetworkLong nb) => new NetworkLong((n - nb._v));
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
        
        public bool SetValue(object o)
        {
            if (o is long v)
            {
                _v = v;
                return true;
            }

            return false;
        }


        public long GetTypedValue()
        {
            return _v;
        }
        object INetworkDeserializable.GetValue()
        {
            return GetTypedValue();
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = BitConverter.ToInt64(bytes, 0); // Read int from bytes.
            return LongByteLength;
        }
        
        public override string ToString()
        {
            return _v.ToString();
        }
    }
}