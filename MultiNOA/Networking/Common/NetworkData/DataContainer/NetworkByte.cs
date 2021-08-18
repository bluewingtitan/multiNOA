using System;

namespace MultiNOA.Networking.Common.NetworkData.DataContainer
{
    /// <summary>
    /// Wraps an Byte to NetworkDataContainer
    /// </summary>
    public struct NetworkByte: INetworkDataContainer<byte>
    {
        private const int ShortByteLength = 2;
        
        private byte _v;

        public NetworkByte(byte value = 0)
        {
            _v = value;
        }
        
        public byte[] TurnIntoBytes()
        {
            return BitConverter.GetBytes(_v);
        }

        public byte GetValue()
        {
            return _v;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = bytes[0]; // Read int from bytes.
            return 1;
        }
    }
}