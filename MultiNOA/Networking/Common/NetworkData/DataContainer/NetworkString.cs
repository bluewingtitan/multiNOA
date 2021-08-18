using System.Collections.Generic;
using System.Text;

namespace MultiNOA.Networking.Common.NetworkData.DataContainer
{
    /// <summary>
    /// Wraps an String to NetworkDataContainer
    /// </summary>
    public class NetworkString: INetworkDataContainer<string>
    {
        private string _v;

        public NetworkString(string value = "")
        {
            _v = value;
        }

        

        public byte[] TurnIntoBytes()
        {
            var bytes = new List<byte>(Encoding.UTF8.GetBytes(_v));
            bytes.AddRange(new NetworkInt(4).TurnIntoBytes());
            return bytes.ToArray();
        }

        public string GetValue()
        {
            return _v;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            var length = new NetworkInt(0);
            var offset = length.LoadFromBytes(bytes);
            var value = Encoding.UTF8.GetString(bytes, offset, length.GetValue());

            _v = value;
            
            return length.GetValue() + offset;
        }
    }
}