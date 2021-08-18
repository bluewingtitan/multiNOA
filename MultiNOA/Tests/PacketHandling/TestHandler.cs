using System;
using MultiNOA.Attributes.PacketHandling;
using MultiNOA.Middleware;
using MultiNOA.Networking.Common.NetworkData.DataContainer;

namespace MultiNOA.Tests.PacketHandling
{
    public class TestHandler
    {
        [PacketHandler(typeof(TestPacketType), "Four", typeof(TestPacketStruct))]
        public static void HandleFour(TestPacketStruct packetStruct)
        {
            MultiNoaLoggingManager.Logger.Information(packetStruct.Integer.GetValue() + "!");
        }
    }

    public struct TestPacketStruct
    {
        public NetworkInt Integer;

        public TestPacketStruct(int testInt)
        {
            Integer = new NetworkInt(testInt);
        }
    }

    public enum TestPacketType
    {
        One,
        Two,
        Three,
        Four
    }
}