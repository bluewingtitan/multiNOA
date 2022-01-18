namespace MultiNoa.Networking.Data.Serializable
{
    /// <summary>
    /// Used inside of packets.
    /// </summary>
    public class SerializableBuffer : INetworkSerializable
    {
        private readonly byte[] _data;

        public SerializableBuffer(byte[] data)
        {
            this._data = data;
        }
        
        
        public byte[] TurnIntoBytes()
        {
            return _data;
        }
    }
}