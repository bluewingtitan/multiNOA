using System;

namespace MultiNoa.Networking.Data.DataContainer
{
    [PacketHandling.DataContainer(typeof(ulong))]
    public struct NetworkUlong : INetworkDataContainer<ulong>
    {
        #region Operators
        public static NetworkUlong operator +(NetworkUlong nb, ulong n) => new NetworkUlong((nb._v + n));
        public static NetworkUlong operator +(ulong n, NetworkUlong nb) => new NetworkUlong((nb._v + n));
        public static NetworkUlong operator -(NetworkUlong nb, ulong n) => new NetworkUlong((nb._v - n));
        public static NetworkUlong operator -(ulong n, NetworkUlong nb) => new NetworkUlong((n - nb._v));
        public static NetworkUlong operator + (NetworkUlong nb) => new NetworkUlong(nb._v);
        public static NetworkUlong operator ++(NetworkUlong nb)
        {
            nb._v++;
            return nb;
        }
        public static NetworkUlong operator --(NetworkUlong nb)
        {
            nb._v--;
            return nb;
        }

        #endregion

        private const int LongByteLength = 8;
        
        private ulong _v;

        public NetworkUlong(ulong value = 0)
        {
            _v = value;
        }
        
        public byte[] TurnIntoBytes()
        {
            return BitConverter.GetBytes(_v);
        }
        
        public bool SetValue(object o)
        {
            if (o is ulong v)
            {
                _v = v;
                return true;
            }

            return false;
        }


        public ulong GetTypedValue()
        {
            return _v;
        }
        object INetworkDeserializable.GetValue()
        {
            return GetTypedValue();
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = BitConverter.ToUInt64(bytes, 0); // Read int from bytes.
            return LongByteLength;
        }
        
        public override string ToString()
        {
            return _v.ToString();
        }

        public void Dispose()
        {
            // nothing to do here.
        }
    }
}