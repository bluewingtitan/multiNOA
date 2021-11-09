namespace MultiNoa.Networking.Data
{
    /// <summary>
    /// Wraps a generic Datatype to be used in a Networking Context, where it needs to be (de-)serialized from/to bytes.
    /// Acts one way: Bytes -> Data
    /// HAVE TO HAVE A PARAMETERLESS CONSTRUCTOR!
    /// </summary>
    public interface INetworkDeserializable<T1>: INetworkDeserializable
    {
        T1 GetTypedValue();
    }

    public interface INetworkDeserializable
    {
        /// <summary>
        /// Loads data from bytes, returns length of the bytes read out data occupied
        /// </summary>
        /// <param name="bytes">Bytearray to read from</param>
        /// <returns>Length of bytes read out</returns>
        int LoadFromBytes(byte[] bytes);

        object GetValue();

        bool SetValue(object o);

    }
}