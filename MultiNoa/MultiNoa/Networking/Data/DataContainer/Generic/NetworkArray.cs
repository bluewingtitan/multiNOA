using System;
using System.Collections.Generic;
using System.Linq;
using MultiNoa.Extensions;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Data.DataContainer.Generic
{
    /// <summary>
    /// Represents an array of any type.
    /// Type has to be derived from INetworkDataContainer or have a INetworkDataContainer-Implementation registered to the DataContainerManager
    /// (bool, byte, float, int, long, short, string will all work)
    /// </summary>
    /// <typeparam name="T">Type of used array</typeparam>
    public struct NetworkArray<T> : INetworkDataContainer<T[]>
    {
        private T[] _v;
        private readonly bool IsDataContainerType => typeof(INetworkDataContainer).IsAssignableFrom(typeof(T));

        public NetworkArray(T[] arr)
        {
            _v = arr;
        }

        public T[] GetTypedValue()
        {
            return _v;
        }

        public int LoadFromBytes(byte[] bytes)
        {
            return IsDataContainerType ? ReadFromBytesDataContainer(bytes) : ReadFromByteValueType(bytes);
        }
        

        public object GetValue()
        {
            return _v;
        }

        public bool SetValue(object o)
        {
            if (o.GetType() != typeof(T[])) return false;
            
            _v = (T[]) o;
            return true;

        }

        public byte[] TurnIntoBytes()
        {
            return IsDataContainerType ? TurnToBytesDataContainer() : TurnToBytesValueType();
        }



        #region Private Functions

        private int ReadFromByteValueType(byte[] bytes)
        {
            var workingCopy = new byte[bytes.Length];
            bytes.CopyTo(workingCopy, 0);

            var nInt = new NetworkInt();
            var bytesReadTotal = 0;
            
            var readBytes = nInt.LoadFromBytes(workingCopy);
            workingCopy = workingCopy.GetSubarray(readBytes, workingCopy.Length - readBytes);

            bytesReadTotal = readBytes;

            var length = nInt.GetTypedValue();
            
            _v = new T[length];

            for (var i = 0; i < length; i++)
            {
                _v[i] = DataContainerManager.ToValueType<T>(workingCopy, out var read);
                workingCopy = workingCopy.GetSubarray(read, workingCopy.Length - read);
                bytesReadTotal += read;
            }
            
            return bytesReadTotal;
        }


        private int ReadFromBytesDataContainer(byte[] bytes)
        {
            var workingCopy = new byte[bytes.Length];
            bytes.CopyTo(workingCopy, 0);

            var nInt = new NetworkInt();
            var bytesReadTotal = 0;
            
            var readBytes = nInt.LoadFromBytes(workingCopy);
            workingCopy = workingCopy.GetSubarray(readBytes, workingCopy.Length - readBytes);

            bytesReadTotal = readBytes;

            var length = nInt.GetTypedValue();
            
            _v = new T[length];

            for (var i = 0; i < length; i++)
            {
                
                T instance = Activator.CreateInstance<T>();

                if (instance is INetworkDataContainer c)
                {
                    readBytes = c.LoadFromBytes(workingCopy);
                    _v[i] = (T) c;
                }
                else
                {
                    throw new Exception($"Wasn't able to read bytes: {typeof(T).FullName} does not implement assumed {typeof(INetworkDataContainer).FullName}");
                }
                
                workingCopy = workingCopy.GetSubarray(readBytes, workingCopy.Length - readBytes);
                bytesReadTotal += readBytes;
            }
            
            return bytesReadTotal;
        }

        
        private byte[] TurnToBytesValueType()
        {
            var bytes = new List<byte>();
            
            // Write length of array
            bytes.AddRange(new NetworkInt(_v.Length).TurnIntoBytes());

            foreach (var t in _v)
            {
                bytes.AddRange( DataContainerManager.ToBytes(t));
            }
            

            return bytes.ToArray();
        }

        private byte[] TurnToBytesDataContainer()
        {
            var bytes = new List<byte>();
            
            // Write length of array
            bytes.AddRange(new NetworkInt(_v.Length).TurnIntoBytes());

            foreach (var t in _v)
            {
                if (t is INetworkDataContainer c)
                {
                    bytes.AddRange(c.TurnIntoBytes());
                }
                else
                {
                    throw new Exception($"Wasn't able to read bytes: {typeof(T).FullName} does not implement assumed {typeof(INetworkDataContainer).FullName}");
                }
            }
            return bytes.ToArray();
        }


        #endregion
        
        
        
    }
}