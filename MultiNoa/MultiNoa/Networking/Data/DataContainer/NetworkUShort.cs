using System;

namespace MultiNoa.Networking.Data.DataContainer
{
    /// <summary>
    /// Wraps an unsigned short to NetworkDataContainer
    /// </summary>
    [PacketHandling.DataContainer(typeof(ushort))]
    public struct NetworkUShort: INetworkDataContainer<ushort>
    {
        
        #region Operators
        public static NetworkUShort operator +(NetworkUShort nb, ushort n) => new NetworkUShort( (ushort) (nb._v + n));
        public static NetworkUShort operator +(ushort n, NetworkUShort nb) => new NetworkUShort( (ushort) (nb._v + n));
        public static NetworkUShort operator -(NetworkUShort nb, ushort n) => new NetworkUShort( (ushort) (nb._v - n));
        public static NetworkUShort operator -(ushort n, NetworkUShort nb) => new NetworkUShort((ushort) (n - nb._v));
        public static NetworkUShort operator + (NetworkUShort nb) => new NetworkUShort(nb._v);
        public static NetworkUShort operator ++(NetworkUShort nb)
        {
            nb._v++;
            return nb;
        }
        public static NetworkUShort operator --(NetworkUShort nb)
        {
            nb._v--;
            return nb;
        }

        #endregion

        private const int ShortByteLength = 2;
        
        private ushort _v;

        public NetworkUShort(ushort value = 0)
        {
            _v = value;
        }
        
        public byte[] TurnIntoBytes()
        {
            return BitConverter.GetBytes(_v);
        }
        
        public bool SetValue(object o)
        {
            if (o is ushort v)
            {
                _v = v;
                return true;
            }

            return false;
        }


        public ushort GetTypedValue()
        {
            return _v;
        }
        object INetworkDeserializable.GetValue()
        {
            return GetTypedValue();
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = BitConverter.ToUInt16(bytes, 0); // Read int from bytes.
            return ShortByteLength;
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