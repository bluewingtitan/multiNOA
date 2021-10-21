using System;
using System.Reflection;
using ExampleProject.Packets;
using MultiNoa.Networking.PacketHandling;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            PacketReflectionHandler.RegisterAssembly(Assembly.GetEntryAssembly());
            PacketConverter.RegisterAssembly(Assembly.GetEntryAssembly());
            
            var msg = new Message(69, "Cool Message!", "Hi Dude! Hope you are doing fine!");
            var bytes = PacketConverter.ObjectToByte(msg);

            PacketReflectionHandler.HandlePacketStatic(bytes, null);
        }
    }
}