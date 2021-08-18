namespace MultiNOA.Networking.Common.NetworkData
{
    /// <summary>
    /// Wraps a generic Datatype to be used in a Networking Context, where it needs to be (de-)serialized from/to bytes.
    /// Acts one way: Bytes -> Data
    /// HAVE TO HAVE A PARAMETERLESS CONSTRUCTOR!
    /// </summary>
    public interface INetworkDeserializable<T1>: INetworkDeserializable
    {
        T1 GetValue();
    }

    public interface INetworkDeserializable
    {
        /// <summary>
        /// Loads data from bytes, has to return the length of the bytes read out
        /// </summary>
        /// <param name="bytes">Bytearray to read from</param>
        /// <param name="startPosition">Position where data starts inside array</param>
        /// <returns>Length of bytes read out</returns>
        int LoadFromBytes(byte[] bytes);
    }
}