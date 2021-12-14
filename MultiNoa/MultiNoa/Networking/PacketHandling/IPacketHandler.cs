using System;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.PacketHandling
{
    /// <summary>
    /// One of the most basic layers of multiNoa dynamic packet handling,
    /// allowing for context-driven packet handling with a lot of freedom and less overhead.
    /// Most people might want to use PacketConverter with the dynamic handling instead.
    /// </summary>
    public interface IPacketHandler
    {
        /// <summary>
        /// Delegate for writing PacketHandlerFunctions, which a single Packet Handler often uses multiple ones,
        /// one for each packet type.
        /// Just a technical example what to use inside a PacketHandler and used inside aton (thus present here).
        /// </summary>
        /// <param name="fromClient">The Client the packet came from</param>
        /// <param name="packet">A packet that has the packet_id already read out</param>
        public delegate void PacketHandleFunction(ConnectionBase fromClient, Packet packet);
        
        
        /// <summary>
        /// Main Function, used for any packet arriving, separated by packets.
        /// </summary>
        /// <param name="packetBytes">Unfiltered bytes the packet delivered</param>
        /// <param name="fromClient">The Client the packet came from</param>
        /// <returns>Was the handling successful or not?</returns>
        public bool HandlePacket(byte[] packetBytes, ConnectionBase fromClient);
        
        /// <summary>
        /// Main Functionality, used for any packet arriving, seperate by packets.
        /// Returns the action to execute to start the actual handling (relevant, thread dependent effects).
        /// All thread indipendent analysis happens before returning the action, not in it.
        /// </summary>
        /// <param name="packetBytes">raw bytes of the received packet</param>
        /// <param name="fromClient">IConnection the bytes came from for context-aware handling</param>
        /// <returns>Action to be executed to start handling</returns>
        public Action PrepareHandling(byte[] packetBytes, ConnectionBase fromClient);
    }
}