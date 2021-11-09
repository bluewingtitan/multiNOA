using System;
using System.Text;
using MultiNoa;
using MultiNoa.Logging;
using MultiNoa.Networking.Data.DataContainer;
using MultiNoa.Networking.PacketHandling;
using NUnit.Framework;

namespace MultiNoaTests.Networking.PacketHandling
{
    public class PacketHandlerTest
    {

        [OneTimeSetUp]
        public void Setup()
        {
            MultiNoaSetup.DefaultSetup(typeof(PacketHandlerTest).Assembly);
        }
        
        /// <summary>
        /// tests basic functionality and sorting by property index
        /// </summary>
        [Test]
        public void DemoStruct1Test()
        {
            var str1 = new DemoStruct1(7,9);
            
            Assert.AreEqual(7, str1.Byte1.GetTypedValue());
            Assert.AreEqual(9, str1.Byte2.GetTypedValue());
            
            var bytes = PacketConverter.ObjectToByte(str1, writeLength:false);

            var str2 = PacketConverter.BytesToObject<DemoStruct1>(bytes);
            
            Assert.AreEqual(str1.Byte1.GetTypedValue(), str2.Byte1.GetTypedValue());
            Assert.AreEqual(str1.Byte2.GetTypedValue(), str2.Byte2.GetTypedValue());

        }

        /// <summary>
        /// tests all basic implemented data containers
        /// </summary>
        [Test]
        public void DemoStruct2Test()
        {
            var str1 = new DemoStruct2("demo!¸´ÜÈ", 71654468L, 2, -2938, 12321);
            var bytes = PacketConverter.ObjectToByte(str1, writeLength:false);
            var str2 = PacketConverter.BytesToObject<DemoStruct2>(bytes);
            
            Assert.AreEqual(str1.Byte1.GetTypedValue(),str2.Byte1.GetTypedValue());
            Assert.AreEqual(str1.Int1.GetTypedValue(),str2.Int1.GetTypedValue());
            Assert.AreEqual(str1.Long1.GetTypedValue(),str2.Long1.GetTypedValue());
            Assert.AreEqual(str1.Short1.GetTypedValue(),str2.Short1.GetTypedValue());
            Assert.AreEqual(str1.String1.GetTypedValue(),str2.String1.GetTypedValue());
        }

        /// <summary>
        /// Tests Dynamic Container Usage (Working with basic value types => string, int, float, ...)
        /// </summary>
        [Test]
        public void DemoStruct3Test()
        {
            var str1 = new DemoStruct3(912309, "demo!¸´ÜÈ", 1.02384f);
            var bytes = PacketConverter.ObjectToByte(str1, writeLength:false);
            var str2 = PacketConverter.BytesToObject<DemoStruct3>(bytes);
            
            Assert.AreEqual(str1.Float1, str2.Float1);
            Assert.AreEqual(str1.Int1, str2.Int1);
            Assert.AreEqual(str1.String1, str2.String1);
        }

        [Test]
        public void ALotOfConversions()
        {
            var str1 = new DemoStruct2("demo!¸´ÜÈ", 71654468L, 2, -2938, 12321);

            for (int i = 0; i < 100000; i++)
            {
                var bytes = PacketConverter.ObjectToByte(str1, writeLength: false);
                var str2 = PacketConverter.BytesToObject<DemoStruct2>(bytes);
            }
        }
        
    }

    [PacketStruct(0)]
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
    
    [PacketStruct(1)]
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
    
    [PacketStruct(2)]
    public struct DemoStruct3
    {
        [NetworkProperty] public int Int1 { get; set; }
        [NetworkProperty] public string String1 { get; set; }
        [NetworkProperty] public float Float1 { get; set; }

        public DemoStruct3(int int1, string string1, float float1)
        {
            Int1 = int1;
            String1 = string1;
            Float1 = float1;
        }
    }
    
}