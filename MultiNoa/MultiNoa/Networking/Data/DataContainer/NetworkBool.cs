namespace MultiNOA.Networking.Common.NetworkData.DataContainer
{
    /// <summary>
    /// Wraps an Byte to NetworkDataContainer
    /// </summary>
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

        public bool GetValue()
        {
            return _v;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = bytes[0] == 1 ? true : false; // Read int from bytes.
            return ByteLength;
        }

        public override string ToString()
        {
            return _v.ToString();
        }
    }
}