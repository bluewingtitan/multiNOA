using System;
using System.Collections.Generic;
using System.Threading;
using MultiNoa.Logging;
using MultiNoa.Networking.ControlPackets;
using MultiNoa.Networking.Data.DataContainer.Generic;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport.Middleware.Fragmentation
{
    [PacketHandler]
    [MultiNoaInternal]
    public class NoaFragmentationMiddleware: INoaMiddleware
    {
        // 3kib
        private const int MaxPacketSize = 3072;
        
        private static NoaFragmentationMiddleware _instance = null;

        public MiddlewareTarget GetTarget() => MiddlewareTarget.Fragmenting;
        
        
        public void Setup()
        {
            MultiNoaLoggingManager.Logger.Information("Using NoaFragmentationMiddleware");
            MultiNoaLoggingManager.Logger.Information("Please be aware that NoaFragmentationMiddleware was designed to be sturdy, not fast. If you need to transfer larger datasets, you might want to change to another framework, as multiNoa does only cover this functionality for reasons of completeness.");
            _instance ??= this;
        }

        public void OnConnectedServerside(ConnectionBase connection){}

        public List<byte> OnSend(List<byte> data, ConnectionBase connection)
        {
            MultiNoaLoggingManager.Logger.Warning($"[{data[0]},{data[1]},{data[2]},{data[3]}]");
            MultiNoaLoggingManager.Logger.Warning($"[{data[4]},{data[5]},{data[6]},{data[7]}]");
            if(data.Count <= MaxPacketSize) return data;

            
            MultiNoaLoggingManager.Logger.Debug("Packet is too big, trying to make it smaller");

            connection.SetMiddlewareDataIfNotPresent(_instance, new PacketFragmentData());
            if (connection.TryGetMiddlewareData(_instance, out var cdata))
            {
                if (cdata is PacketFragmentData pf)
                {
                    var id = pf.GetNewId();
                    connection.SetMiddlewareData(_instance, pf);
                    SendPacketInFragments(data, connection, id);
            
                    return new List<byte>();
                }
            }
            
            MultiNoaLoggingManager.Logger.Warning("Dropped packet: Packet would have been to long and NoaFragmentationManager wasn't initialized properly! Stack Trace appended\n" + Environment.StackTrace);
            
            return new List<byte>();
            
        }


        private static void SendPacketInFragments(List<byte> d, ConnectionBase c, ulong seriesId)
        {
            var packetsToSend = (int) Math.Ceiling((double) d.Count / MaxPacketSize);
            var sizePerPacket = (int) Math.Ceiling((double) d.Count / packetsToSend);
            
            MultiNoaLoggingManager.Logger.Debug($"Split up packet into {packetsToSend} packets with {sizePerPacket}b each.");

            for (int i = 0; i < packetsToSend; i++)
            {
                var readAt = sizePerPacket * i;
                var toSend = d.GetRange(readAt, Math.Min(sizePerPacket, ((d.Count - 1) - readAt)));
                
                c.SendData(new FragmentPacket
                {
                    SeriesId = seriesId,
                    PacketIndex = i,
                    NumberOfPackets = packetsToSend,
                    FixedPacketByteLength = sizePerPacket,
                    Bytes = toSend.ToArray()
                },false, true, new [] {MiddlewareTarget.Fragmenting});
                
                // As mentioned above, in the warning printed in SetUp(),
                // this middleware is designed to work, not to be fast.
                // It's not a main feature, but a last resort for situations where it's needed
                // To be reliable, it needs to add some time before sending, otherwise it won't behave reliable on *a lot* of data.
                
                // I could increase the input buffer, but that would result in too many problems when using multiNoa in it's INTENDED way.
                Thread.Sleep(5);
            }
        }

        
        // Don't do anything special, packet handling will do everything needed.
        public List<byte> OnReceive(List<byte> data, ConnectionBase connection)
        {
            return data;
        }


        private class PacketFragmentData
        {
            private ulong _currentId = 0;
            private readonly object _lock = '0';

            public readonly Dictionary<ulong, byte[]> UnfinishedPackets =
                new Dictionary<ulong, byte[]>();

            private readonly Dictionary<ulong, int> _receivedPackets = new Dictionary<ulong, int>();

            public int RegisterReceivedPacket(ulong seriesId)
            {
                var output = 0;
                
                if (!_receivedPackets.ContainsKey(seriesId))
                    _receivedPackets[seriesId] = 0;
                    
                output = _receivedPackets[seriesId];
                output += 1;
                _receivedPackets[seriesId] = output;

                return output;
            }

            public void RemoveReceivePacketCounter(ulong seriesId) => _receivedPackets.Remove(seriesId);
            
            
            public ulong GetNewId()
            {
                var newValue = (ulong) 0;
                lock (_lock)
                {
                    newValue = _currentId++;
                }
                return newValue;
            }

        }
        
        [HandlerMethod(NoaControlPacketIds.Symmetrical.FragmentPacket)]
        public static void HandleFragmentPacket(FragmentPacket p, ConnectionBase c)
        {
            MultiNoaLoggingManager.Logger.Warning("LL!");
            
            c.SetMiddlewareDataIfNotPresent(_instance, new PacketFragmentData());
            
            if (!c.TryGetMiddlewareData(_instance, out var cdata)) return;
            if (!(cdata is PacketFragmentData pf)) return;

            var receivedPackets = 0;
            var bytes = new byte[0];
            
            lock (pf) // only ever work on a single packet at once!
            {
                if (!pf.UnfinishedPackets.ContainsKey(p.SeriesId))
                {
                    pf.UnfinishedPackets[p.SeriesId] = new byte[p.NumberOfPackets*p.FixedPacketByteLength];
                }

                var data = pf.UnfinishedPackets[p.SeriesId];

                var newData = p.Bytes;
                var index = p.PacketIndex * p.FixedPacketByteLength;

                // Write into receive buffer
                for (int i = index; i < index+newData.Length; i++)
                {
                    data[i] = newData[i - index];
                }
                

                pf.UnfinishedPackets[p.SeriesId] = data;

                receivedPackets = pf.RegisterReceivedPacket(p.SeriesId);
                
                MultiNoaLoggingManager.Logger.Debug($"Received packet #{p.PacketIndex}, {p.NumberOfPackets - receivedPackets} packets missing");
                
                c.SetMiddlewareData(_instance, pf);

                if (receivedPackets == p.NumberOfPackets)
                {
                    // => this was the last packet in the series! => Save results and remove pointers!
                    bytes = data;
                    pf.UnfinishedPackets.Remove(p.SeriesId);
                    pf.RemoveReceivePacketCounter(p.SeriesId);
                }
            }
            
            if (receivedPackets == p.NumberOfPackets)
            {
                // => this was the last packet in the series! => Initialize parsing!
                PacketReflectionHandler.HandlePacketStatic(NoaMiddlewareManager.OnReceive(new List<byte>(bytes), c).ToArray(), c);
            }
        }
        
        [MultiNoaInternal]
        [PacketStruct(NoaControlPacketIds.Symmetrical.FragmentPacket)]
        public struct FragmentPacket
        {
            [NetworkProperty(0)] public int PacketIndex { get; set; }
            [NetworkProperty(1)] public int NumberOfPackets { get; set; }
            [NetworkProperty(2)] public int FixedPacketByteLength { get; set; }
            [NetworkProperty(3)] public ulong SeriesId { get; set; }
            [NetworkProperty(4)] public byte[] Bytes { get; set; }
        }
        
        
    }
}