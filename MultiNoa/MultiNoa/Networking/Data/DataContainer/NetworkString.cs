using System;
using System.Collections.Generic;
using System.Text;

namespace MultiNOA.Networking.Common.NetworkData.DataContainer
{
    /// <summary>
    /// Wraps an String to NetworkDataContainer
    /// </summary>
    [MultiNoa.Networking.PacketHandling.DataContainer(typeof(string))]
    public struct NetworkString: INetworkDataContainer<string>
    {
        #region Operators
        public static NetworkString operator +(NetworkString nb, string s) => new NetworkString(nb._v + s);
        public static NetworkString operator +(string s, NetworkString nb) => new NetworkString(s + nb._v);
        public static NetworkString operator +(NetworkString nb2, NetworkString nb) => new NetworkString(nb2._v + nb._v);

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
        
        public bool SetValue(object o)
        {
            if (o is string v)
            {
                _v = v;
                return true;
            }

            return false;
        }


        public string GetTypedValue()
        {
            return _v;
        }
        object INetworkDeserializable.GetValue()
        {
            return GetTypedValue();
        }

        public int LoadFromBytes(byte[] bytes)
        {
            var length = new NetworkInt(0);
            var offset = length.LoadFromBytes(bytes);
            var value = Encoding.UTF8.GetString(bytes, offset, length.GetTypedValue());

            _v = value;
            
            return length.GetTypedValue() + offset;
        }

        public override string ToString()
        {
            return _v;
        }
    }
}