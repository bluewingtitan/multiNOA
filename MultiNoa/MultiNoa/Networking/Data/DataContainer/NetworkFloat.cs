using System;
using System.Globalization;

namespace MultiNoa.Networking.Data.DataContainer
{
    [MultiNoa.Networking.PacketHandling.DataContainer(typeof(float))]
    public struct NetworkFloat: INetworkDataContainer<float>
    {
        
        #region Operators
        public static NetworkFloat operator +(NetworkFloat nb, float n) => new NetworkFloat((nb._v + n));
        public static NetworkFloat operator +(float n, NetworkFloat nb) => new NetworkFloat((nb._v + n));
        public static NetworkFloat operator -(NetworkFloat nb, float n) => new NetworkFloat((nb._v - n));
        public static NetworkFloat operator -(float n, NetworkFloat nb) => new NetworkFloat((n - nb._v));
        public static NetworkFloat operator + (NetworkFloat nb) => new NetworkFloat(nb._v);
        public static NetworkFloat operator ++(NetworkFloat nb)
        {
            nb._v++;
            return nb;
        }
        public static NetworkFloat operator --(NetworkFloat nb)
        {
            nb._v--;
            return nb;
        }

        #endregion

        private const int FloatByteLength = 4;
        
        private float _v;

        public NetworkFloat(float value = 0)
        {
            _v = value;
        }
        
        public byte[] TurnIntoBytes()
        {
            return BitConverter.GetBytes(_v);
        }
        
        public bool SetValue(object o)
        {
            if (o is float v)
            {
                _v = v;
                return true;
            }

            return false;
        }

        public float GetTypedValue()
        {
            return _v;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            _v = BitConverter.ToSingle(bytes, 0); // Read int from bytes.
            return FloatByteLength;
        }

        object INetworkDeserializable.GetValue()
        {
            return GetTypedValue();
        }

        public override string ToString()
        {
            return _v.ToString(CultureInfo.CurrentCulture);
        }

        public void Dispose()
        {
            // nothing to do here.
        }
    }
}