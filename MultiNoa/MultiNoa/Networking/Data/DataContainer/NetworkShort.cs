using System;

namespace MultiNOA.Networking.Common.NetworkData.DataContainer
{
    /// <summary>
    /// Wraps an Short to NetworkDataContainer
    /// </summary>
    public struct NetworkShort: INetworkDataContainer<short>
    {
        private const int ShortByteLength = 2;
        
        private short _v;

        public NetworkShort(short value = 0)
        {
            _v = value;
        }
        
        public byte[] TurnIntoBytes()
        {
            return BitConverter.GetBytes(_v);
        }

        public short GetValue()
        {
            return _v;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = BitConverter.ToInt16(bytes, 0); // Read int from bytes.
            return ShortByteLength;
        }
    }
}