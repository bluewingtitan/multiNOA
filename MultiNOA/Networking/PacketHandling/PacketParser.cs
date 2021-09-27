using System;
using System.Linq;
using MultiNOA.Networking.Common;
using MultiNOA.Networking.Common.NetworkData;

namespace MultiNOA.Networking.PacketHandling
{
    /// <summary>
    /// Used to parse structs from/to Packets.
    /// </summary>
    public static class PacketParser
    {
        public static T1 ParseToObject<T1>(Packet packet)
        {
            var fields = typeof(T1).GetFields().Where(info => info.FieldType.IsAssignableFrom(typeof(INetworkDataContainer))).ToArray();

            var @object = Activator.CreateInstance<T1>();
            
            
            foreach (var field in fields)
            {
                var t = field.FieldType;
                ((INetworkDataContainer)field.GetValue(@object)).LoadFromBytes( ((INetworkDataContainer)packet.Read<t>()).TurnIntoBytes());
            }

            return packet;
        }


        public static Packet ParseToPacket(object @object, int packetType)
        {
            var t = @object.GetType();
            var fields = t.GetFields().Where(info => info.FieldType.IsAssignableFrom(typeof(INetworkDataContainer))).ToArray();

            var packet = new Packet(packetType);
            
            foreach (var field in fields)
            {
                packet.Write(((INetworkDataContainer)field.GetValue(@object)));
            }

            return packet;
        }
        
    }
}