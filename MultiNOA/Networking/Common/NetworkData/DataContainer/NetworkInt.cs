using System;

namespace MultiNOA.Networking.Common.NetworkData.DataContainer
{
    /// <summary>
    /// Wraps an Integer to NetworkDataContainer
    /// </summary>
    public struct NetworkInt: INetworkDataContainer<int>
    {
        private const int IntegerByteLength = 4;
        
        private int _v;

        public NetworkInt(int value = 0)
        {
            _v = value;
        }
        
        public byte[] TurnIntoBytes()
        {
            return BitConverter.GetBytes(_v);
        }

        public int GetValue()
        {
            return _v;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = BitConverter.ToInt32(bytes, 0); // Read int from bytes.
            return IntegerByteLength;
        }
    }
}