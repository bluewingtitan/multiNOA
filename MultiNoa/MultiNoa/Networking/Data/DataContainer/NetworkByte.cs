namespace MultiNoa.Networking.Data.DataContainer
{
    /// <summary>
    /// Wraps an Byte to NetworkDataContainer
    /// </summary>
    [MultiNoa.Networking.PacketHandling.DataContainer(typeof(byte))]
    public struct NetworkByte: INetworkDataContainer<byte>
    {
        #region Operators
        public static NetworkByte operator +(NetworkByte nb, byte n) => new NetworkByte((byte) (nb._v + n));
        public static NetworkByte operator +(byte n, NetworkByte nb) => new NetworkByte((byte) (nb._v + n));
        public static NetworkByte operator -(NetworkByte nb, byte n) => new NetworkByte((byte) (nb._v - n));
        public static NetworkByte operator -(byte n,NetworkByte nb) => new NetworkByte((byte) (n - nb._v));
        public static NetworkByte operator + (NetworkByte nb) => new NetworkByte(nb._v);
        public static NetworkByte operator ++(NetworkByte nb)
        {
            nb._v++;
            return nb;
        }

        #endregion
        
        
        private const int ByteLength = 1;
        
        private byte _v;

        public NetworkByte(byte value = 0)
        {
            _v = value;
        }
        
        public byte[] TurnIntoBytes()
        {
            return new []{_v};
        }
        
        public bool SetValue(object o)
        {
            if (o is byte v)
            {
                _v = v;
                return true;
            }

            return false;
        }

        public byte GetTypedValue()
        {
            return _v;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = bytes[0]; // Read int from bytes.
            return ByteLength;
        }

        object INetworkDeserializable.GetValue()
        {
            return GetTypedValue();
        }

        public override string ToString()
        {
            return _v.ToString();
        }
    }
}