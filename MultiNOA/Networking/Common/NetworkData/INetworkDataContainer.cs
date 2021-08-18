namespace MultiNOA.Networking.Common.NetworkData
{
    /// <summary>
    /// Wraps a generic Datatype to be used in a Networking Context, where it needs to be (de-)serialized from/to bytes.
    /// Acts two-way (Bytes -> Data -> Bytes)
    /// </summary>
    public interface INetworkDataContainer<T1>: INetworkSerializable, INetworkDeserializable<T1>
    {
        
    }
}