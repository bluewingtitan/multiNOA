using System;
using System.Text;
using MultiNoa.Logging;
using MultiNOA.Networking.Common.NetworkData.DataContainer;
using MultiNoa.Networking.PacketHandling;
using NUnit.Framework;

namespace MultiNoaTests.Networking.PacketHandling
{
    public class PacketHandlerTest
    {

        [OneTimeSetUp]
        public void Setup()
        {
            PacketConverter.CachePacketStructure(typeof(DemoStruct1));
            PacketConverter.CachePacketStructure(typeof(DemoStruct2));
        }
        
        [Test]
        public void DemoStruct1Test()
        {
            var str1 = new DemoStruct1(7,9);
            
            Assert.AreEqual(7, str1.Byte1.GetValue());
            Assert.AreEqual(9, str1.Byte2.GetValue());
            
            var bytes = PacketConverter.ObjectToByte(str1);
            
            Assert.AreEqual(new byte[]{7,9}, bytes);

            var str2 = PacketConverter.BytesToObject<DemoStruct1>(bytes);
            
            Assert.AreEqual(str1.Byte1.GetValue(), str2.Byte1.GetValue());
            Assert.AreEqual(str1.Byte2.GetValue(), str2.Byte2.GetValue());

        }

        [Test]
        public void DemoStruct2Test()
        {
            var str1 = new DemoStruct2("demo!¸´ÜÈ", 71654468L, 2, -2938, 12321);
            var bytes = PacketConverter.ObjectToByte(str1);
            var str2 = PacketConverter.BytesToObject<DemoStruct2>(bytes);
            
            Assert.AreEqual(str1.Byte1.GetValue(),str2.Byte1.GetValue());
            Assert.AreEqual(str1.Int1.GetValue(),str2.Int1.GetValue());
            Assert.AreEqual(str1.Long1.GetValue(),str2.Long1.GetValue());
            Assert.AreEqual(str1.Short1.GetValue(),str2.Short1.GetValue());
            Assert.AreEqual(str1.String1.GetValue(),str2.String1.GetValue());
        }

        [Test]
        public void ALotOfConversions()
        {
            var str1 = new DemoStruct2("demo!¸´ÜÈ", 71654468L, 2, -2938, 12321);

            for (int i = 0; i < 1000000; i++)
            {
                var bytes = PacketConverter.ObjectToByte(str1);
                var str2 = PacketConverter.BytesToObject<DemoStruct2>(bytes);
            }
        }
        
    }

    [PacketClass(0)]
    public struct DemoStruct1
    {
        [NetworkProperty(9)]
        public NetworkByte Byte2 { get; private set; }
        
        [NetworkProperty(4)]
        public NetworkByte Byte1 { get; private set; }

        public DemoStruct1(byte b1, byte b2)
        {
            Byte1 = new NetworkByte(b1);
            Byte2 = new NetworkByte(b2);
        }
        
    }
    
    [PacketClass(1)]
    public struct DemoStruct2
    {
        [NetworkProperty] public NetworkString String1 { get; private set; }
        [NetworkProperty] public NetworkLong Long1 { get; private set; }
        [NetworkProperty] public NetworkByte Byte1 { get; private set; }
        [NetworkProperty] public NetworkInt Int1 { get; private set; }
        [NetworkProperty] public NetworkShort Short1 { get; private set; }

        public DemoStruct2(string s1, long l1, byte b1, int i1, short sh1)
        {
            String1 = new NetworkString(s1);
            Long1 = new NetworkLong(l1);
            Byte1 = new NetworkByte(b1);
            Int1 = new NetworkInt(i1);
            Short1 = new NetworkShort(sh1);
        }
        
        
    }
    
}