using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.PacketHandling
{
    /// <summary>
    /// One of the most basic layers of multiNoa dynamic packet handling,
    /// allowing for context-driven packet handling
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
        public delegate void PacketHandleFunction(ITcpConnetion fromClient, Packet packet);
        
        
        /// <summary>
        /// Main Function, used for any packet arriving, separated by packets.
        /// </summary>
        /// <param name="packetBytes">Unfiltered bytes the packet delivered</param>
        /// <param name="fromClient">The Client the packet came from</param>
        /// <returns>Was the handling successful or not?</returns>
        public bool HandlePacket(byte[] packetBytes, ITcpConnetion fromClient);
    }
}