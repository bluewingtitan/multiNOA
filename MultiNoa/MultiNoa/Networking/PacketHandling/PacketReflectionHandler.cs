using System;
using System.Collections.Concurrent;
using System.Reflection;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.PacketHandling
{
    /// <summary>
    /// Handles Packets in a Reflective Manner.
    /// </summary>
    public class PacketReflectionHandler: IPacketHandler
    {
        // TODO: Cache PacketHandler-Classes


        private static readonly ConcurrentDictionary<int, PacketClassInfo> Infos = new ConcurrentDictionary<int, PacketClassInfo>();

        public bool HandlePacket(byte[] b, IConnection fromClient)
        {
            var o = PacketConverter.BytesToObject(b);
            
            var type = o.GetType();
            
            if (!(type.GetCustomAttribute(typeof(PacketClass)) is PacketClass attribute))
            {
                throw new PacketConversionException($"Type {type} does not contain PacketClass Attribute");
            }


            return true;
        }
    }


    internal class PacketHandlerInfo
    {
        
    }
    
    
    
    [AttributeUsage(AttributeTargets.Method)]
    public class PacketHandler : Attribute
    {
        public readonly int PacketId;
        public PacketHandler(int packetId)
        {
            PacketId = packetId;
        }
    }
    
    
    
}