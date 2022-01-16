namespace MultiNoa.Networking.Data.DataContainer
{
    /// <summary>
    /// Wraps an Byte to NetworkDataContainer
    /// </summary>
    [MultiNoa.Networking.PacketHandling.DataContainer(typeof(bool))]
    public struct NetworkBool: INetworkDataContainer<bool>
    {
        private const int ByteLength = 1;
        
        private bool _v;

        public NetworkBool(bool value = false)
        {
            _v = value;
        }
        
        public byte[] TurnIntoBytes()
        {
            return new []{(byte) (_v?1:0)};
        }

        public bool GetTypedValue()
        {
            return _v;
        }

        public bool SetValue(object o)
        {
            if (o is bool b)
            {
                _v = b;
                return true;
            }

            return false;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = bytes[0] == 1 ? true : false; // Read int from bytes.
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

        public void Dispose()
        {
            // nothing to do here.
        }
    }
}