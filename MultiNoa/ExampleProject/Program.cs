using System;
using System.Reflection;
using ExampleProject.Packets;
using MultiNoa;
using MultiNoa.Networking.PacketHandling;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            MultiNoaSetup.DefaultSetup(typeof(Program).Assembly);

            var msg = new Message(-42069, "——Cool Message——", "Hi Dude! Hope you are doing   𝖋𝖎𝖓𝖊!   😲");
                                                          var bytes = PacketConverter.ObjectToByte(msg);

            PacketReflectionHandler.HandlePacketStatic(bytes, null);
        }
    }
}