using System.Collections.Generic;
using MultiNoa.Extensions;
using MultiNoa.Networking.Data.DataContainer;

namespace MultiNoa.Networking.Data.Serializable
{
    /// <summary>
    /// Represents a byte array of a fixed length
    /// </summary>
    [MultiNoa.Networking.PacketHandling.DataContainer(typeof(byte[]))]
    public struct NetworkByteArray : INetworkDataContainer<byte[]>
    {
        private byte[] _data;

        public NetworkByteArray(byte[] data)
        {
            _data = data;
        }
        
        
        public byte[] TurnIntoBytes()
        {
            var returnData = new List<byte>(_data);
            returnData.InsertRange(0, new NetworkInt(_data.Length).TurnIntoBytes());
            return returnData.ToArray();
        }

        public byte[] GetTypedValue()
        {
            return _data;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            var nInt = new NetworkInt();
            var rl = nInt.LoadFromBytes(bytes);
            _data = bytes.GetSubarray(rl, nInt.GetTypedValue()); // after length-int ended, read array into value.

            return nInt.GetTypedValue() + rl; // array size + int size
        }

        public object GetValue()
        {
            return _data;
        }

        public bool SetValue(object o)
        {
            if (!(o is byte[] b)) return false;
            
            _data = b;
            return true;
        }
    }
}