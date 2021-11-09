namespace MultiNoa.Networking.Data.Serializable
{
    /// <summary>
    /// Represents a byte array of a fixed length
    /// </summary>
    public class NetworkByteArray : INetworkSerializable
    {
        private readonly byte[] data;

        public NetworkByteArray(byte[] data)
        {
            this.data = data;
        }
        
        
        public byte[] TurnIntoBytes()
        {
            return data;
        }
    }
}