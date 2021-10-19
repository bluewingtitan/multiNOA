using System;
using System.Collections.Generic;
using System.Text;

namespace MultiNOA.Networking.Common.NetworkData.DataContainer
{
    /// <summary>
    /// Wraps an String to NetworkDataContainer
    /// </summary>
    public struct NetworkString: INetworkDataContainer<string>
    {
        #region Operators
        public static NetworkString operator +(NetworkString nb, string s) => new NetworkString(nb._v + s);
        public static NetworkString operator +(string s, NetworkString nb) => new NetworkString(s + nb._v);

        #endregion

        private string _v;

        public NetworkString(string value = "")
        {
            _v = value;
        }

        

        public byte[] TurnIntoBytes()
        {
            var strBytes = Encoding.UTF8.GetBytes(_v);
            var bytes = new List<byte>(new NetworkInt(strBytes.Length).TurnIntoBytes());
            bytes.AddRange(strBytes);
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