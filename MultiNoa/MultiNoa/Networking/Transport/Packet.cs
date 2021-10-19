using System;
using System.Collections.Generic;
using MultiNoa.Extensions;
using MultiNOA.Networking.Common.NetworkData;

namespace MultiNoa.Networking.Transport
{
    /// <summary>
    /// Represents a message that can be transmitted via networking.
    /// Highly inspired by Tom Weilands Packet Class in functionality, but designed to be way more flexible
    /// </summary>
    public class Packet
    {
        private List<byte> _buffer;
        private byte[] _readableBuffer;
        private int _readPos = 0;

        public Packet()
        {
            _buffer = new List<byte>();
        }

        /// <summary>
        /// Creates a packet of a specific packet type.
        /// </summary>
        /// <param name="packetType">Type of the packet</param>
        public Packet(int type)
        {
            _buffer = new List<byte>();
            
        }

        /// <summary>
        /// Creates a packet in read-only mode with pre-defined data.
        /// Used to read out data from a received byte-array.
        /// </summary>
        /// <param name="data">Data to set</param>
        public Packet(byte[] data)
        {
            SetBytes(data);
            Finalized = true;
        }
        
        /// <summary>
        /// Whether the packet is finalized and in read-only mode now
        /// </summary>
        public bool Finalized { get; private set; } = false;
        
        /// <summary>
        /// Writes the length of the packet and turns it into read-only mode
        /// </summary>
        public void Lock()
        {
            if(Finalized) return; // Only write length once!
            Finalized = true;
            
            _buffer.InsertRange(0, BitConverter.GetBytes(_buffer.Count)); // Insert the byte length of the packet at the very beginning
        }
        
        /// <summary>
        /// Adds byte array to packet contents.
        /// For internal use only.
        /// </summary>
        /// <param name="_data">Bytes to add</param>
        private void SetBytes(byte[] data)
        {
            Write(new NetworkByteArray(data));
            _readableBuffer = _buffer.ToArray();
        }

        public void Write(INetworkSerializable dataContainer)
        {
            if (Finalized)
            {
                throw new Exception("Tried to write into finalized and locked package.");
            }
            _buffer.AddRange(dataContainer.TurnIntoBytes());
        }

        
        public T1 Read<T1>(bool moveReadPos = true) where T1 : INetworkDeserializable
        {
            try
            {
                var instance = (T1) Activator.CreateInstance(typeof(T1));

                var workingCopy = _readableBuffer.GetSubarray(_readPos, _readableBuffer.Length - _readPos);
                var length = instance.LoadFromBytes(workingCopy);
                
                if (moveReadPos && length > 0)
                {
                    // If _moveReadPos is true string is not empty
                    _readPos += length; // Increase readPos by the length of the string
                }
                return instance; // Return the string
            }
            catch
            {
                throw new Exception($"Could not read value of type '{typeof(T1).Name}'!");
            }
        }

        
        
        
    }
}