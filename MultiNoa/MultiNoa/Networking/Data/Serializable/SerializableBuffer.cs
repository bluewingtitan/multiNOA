namespace MultiNoa.Networking.Data.Serializable
{
    /// <summary>
    /// Used inside of packets.
    /// </summary>
    public class SerializableBuffer : INetworkSerializable
    {
        private readonly byte[] data;

        public SerializableBuffer(byte[] data)
        {
            this.data = data;
        }
        
        
        public byte[] TurnIntoBytes()
        {
            return data;
        }
    }
}