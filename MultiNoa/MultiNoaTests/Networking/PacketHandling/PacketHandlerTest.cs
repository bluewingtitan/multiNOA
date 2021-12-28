using MultiNoa;
using MultiNoa.Networking.Data;
using MultiNoa.Networking.Data.DataContainer;
using MultiNoa.Networking.Data.DataContainer.Generic;
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
        public void TestBaseFunctionality()
        {
            var str1 = new DemoStruct1(7,9);
            
            Assert.AreEqual(7, str1.Byte1.GetTypedValue());
            Assert.AreEqual(9, str1.Byte2.GetTypedValue());
            
            var bytes = PacketConverter.ObjectToByte(str1, writeLength:false);

            var str2 = PacketConverter.BytesToObject<DemoStruct1>(bytes.ToArray(), out var dummy);
            
            Assert.AreEqual(str1.Byte1.GetTypedValue(), str2.Byte1.GetTypedValue());
            Assert.AreEqual(str1.Byte2.GetTypedValue(), str2.Byte2.GetTypedValue());
        }

        /// <summary>
        /// tests all basic implemented data containers
        /// </summary>
        [Test]
        public void TestBasicDataContainers()
        {
            var str1 = new DemoStruct2("demo!¸´ÜÈ", 71654468L, 2, -2938, 12321);
            var bytes = PacketConverter.ObjectToByte(str1, writeLength:false);
            var byteArray = bytes.ToArray();
            var str2 = PacketConverter.BytesToObject<DemoStruct2>(byteArray, out var dummy);
            var str3 = PacketConverter.BytesToObject(byteArray);
            
            Assert.AreEqual(str2, str3);
            
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
        public void TestDynamicContainerUsage()
        {
            var str1 = new DemoStruct3(912309, "demo!¸´ÜÈ", 1.02384f);
            var bytes = PacketConverter.ObjectToByte(str1, writeLength:false);
            var str2 = PacketConverter.BytesToObject<DemoStruct3>(bytes.ToArray(), out var dummy);
            
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
                var str2 = PacketConverter.BytesToObject<DemoStruct2>(bytes.ToArray(), out var dummy);
            }
        }

        [Test]
        public void TestGenerics()
        {
            var str1 = new TestGenericsStruct()
            {
                StringNetworkArray = new NetworkArray<string>(new[] {"Test", "demo! ´ÜÈ","!!!"}),
                
                NetworkIntNetworkArray = new NetworkArray<NetworkInt>(new[]
                    {new NetworkInt(177), new NetworkInt(375), new NetworkInt(-500)})
                    
            };

            var bytes = PacketConverter.ObjectToByte(str1, writeLength:false);
            
            var str2 = PacketConverter.BytesToObject<TestGenericsStruct>(bytes.ToArray(), out var dummy);
            
            
            Assert.AreEqual(str1.StringNetworkArray.GetTypedValue()[0], str2.StringNetworkArray.GetTypedValue()[0]);
            Assert.AreEqual(str1.StringNetworkArray.GetTypedValue()[1], str2.StringNetworkArray.GetTypedValue()[1]);
            Assert.AreEqual(str1.StringNetworkArray.GetTypedValue().Length, str2.StringNetworkArray.GetTypedValue().Length);
            
            Assert.AreEqual(str1.NetworkIntNetworkArray.GetTypedValue()[0], str2.NetworkIntNetworkArray.GetTypedValue()[0]);
            Assert.AreEqual(str1.NetworkIntNetworkArray.GetTypedValue()[1], str2.NetworkIntNetworkArray.GetTypedValue()[1]);
            Assert.AreEqual(str1.NetworkIntNetworkArray.GetTypedValue()[2], str2.NetworkIntNetworkArray.GetTypedValue()[2]);
            Assert.AreEqual(str1.NetworkIntNetworkArray.GetTypedValue().Length, str2.NetworkIntNetworkArray.GetTypedValue().Length);
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


    [PacketStruct(3)]
    public struct TestGenericsStruct
    {
        [NetworkProperty]
        public NetworkArray<string> StringNetworkArray { get; set; }

        [NetworkProperty]
        public NetworkArray<NetworkInt> NetworkIntNetworkArray { get; set; }
    }
}