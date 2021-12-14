using System.Collections.Concurrent;
using System.Collections.Generic;
using MultiNoa.Networking.ControlPackets;
using MultiNoa.Networking.Data.DataContainer.Generic;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport.Middleware.Fragmentation
{
    [PacketHandler]
    [MultiNoaInternal]
    public class NoaFragmentationMiddleware: INoaMiddleware
    {
        // 3.5kib
        private const int MaxPacketSize = 3584;
        
        private static NoaFragmentationMiddleware _instance = null;

        public bool DoesModify() => true;
        
        
        public void Setup()
        {
            _instance ??= this;
        }

        public void OnConnectedServerside(ConnectionBase connection)
        {
            connection.SetMiddlewareData(_instance, new PacketFragments());
        }

        public List<byte> OnSend(List<byte> data, ConnectionBase connection)
        {
            
            
            return data;
        }

        
        // Don't do anything special, packet handling will do everything needed.
        public List<byte> OnReceive(List<byte> data, ConnectionBase connection) => data;


        private class PacketFragments
        {
            
        }
        
        [HandlerMethod(NoaControlPacketIds.Symmetrical.FragmentPacket)]
        public static void HandleFragmentPacket(FragmentPacket p, ConnectionBase c)
        {
            
        }
        
        
        [PacketStruct(NoaControlPacketIds.Symmetrical.FragmentPacket)]
        public struct FragmentPacket
        {
            [NetworkProperty(0)] public int PacketIndex { get; set; }
            [NetworkProperty(1)] public int LastIndex { get; set; }
            [NetworkProperty(2)] public int FixedPacketByteLength { get; set; }
            [NetworkProperty(3)] public ulong SeriesId { get; set; }
            [NetworkProperty(4)] public NetworkArray<byte> Bytes { get; set; }
        }
        
        
    }
}