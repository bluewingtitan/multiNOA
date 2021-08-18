namespace MultiNOA.Networking.Common.NetworkData
{
    /// <summary>
    /// Wraps a generic Datatype to be used in a Networking Context, where it needs to be (de-)serialized from/to bytes.
    /// Acts one-way: Data -> Bytes
    /// </summary>
    public interface INetworkSerializable
    {
        /// <summary>
        /// Turns this INetworkSerializable into bytes.
        /// </summary>
        /// <returns></returns>
        byte[] TurnIntoBytes();
    }
}