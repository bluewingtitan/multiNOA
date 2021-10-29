using System;

namespace MultiNOA.Networking.Common.NetworkData.DataContainer
{
    /// <summary>
    /// Wraps an Short to NetworkDataContainer
    /// </summary>
    [MultiNoa.Networking.PacketHandling.DataContainer(typeof(short))]
    public struct NetworkShort: INetworkDataContainer<short>
    {
        #region Operators
        public static NetworkShort operator +(NetworkShort nb, short n) => new NetworkShort( (short) (nb._v + n));
        public static NetworkShort operator +(short n, NetworkShort nb) => new NetworkShort( (short) (nb._v + n));
        public static NetworkShort operator -(NetworkShort nb, short n) => new NetworkShort( (short) (nb._v - n));
        public static NetworkShort operator -(short n, NetworkShort nb) => new NetworkShort((short) (n - nb._v));
        public static NetworkShort operator + (NetworkShort nb) => new NetworkShort(nb._v);
        public static NetworkShort operator ++(NetworkShort nb)
        {
            nb._v++;
            return nb;
        }
        public static NetworkShort operator --(NetworkShort nb)
        {
            nb._v--;
            return nb;
        }

        #endregion

        private const int ShortByteLength = 2;
        
        private short _v;

        public NetworkShort(short value = 0)
        {
            _v = value;
        }
        
        public byte[] TurnIntoBytes()
        {
            return BitConverter.GetBytes(_v);
        }
        
        public bool SetValue(object o)
        {
            if (o is short v)
            {
                _v = v;
                return true;
            }

            return false;
        }


        public short GetTypedValue()
        {
            return _v;
        }
        object INetworkDeserializable.GetValue()
        {
            return GetTypedValue();
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = BitConverter.ToInt16(bytes, 0); // Read int from bytes.
            return ShortByteLength;
        }
        
        public override string ToString()
        {
            return _v.ToString();
        }
    }
}