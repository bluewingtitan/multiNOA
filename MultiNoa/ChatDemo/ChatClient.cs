using System;
using System.Threading;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Rooms;
using MultiNoa.Networking.Server;
using MultiNoa.Networking.Transport;
using MultiNoa.Networking.Transport.Connection;

namespace ChatDemo
{
    public class ChatClient: ClientBase
    {
        protected readonly ServerBase Server;
        protected readonly TcpConnection Connection;
        protected readonly ulong ClientId = 0;


        public void StartTyping()
        {
            var exited = false;
            Console.WriteLine("Type 'exit' to exit the chat.");

            while (!exited)
            {
                var newInput = Console.ReadLine();
                if(String.IsNullOrEmpty(newInput))
                    continue;
                
                if (newInput.Equals("exit"))
                {
                    exited = true;
                    continue;
                }
                
                // Send message
                var msg = new ChatPackets.FromClient.MessageFromClient
                {
                    Message = newInput
                };
                Connection.SendData(PacketConverter.ObjectToByte(msg));
            }
            
            Console.WriteLine("Thank you for using NoaChat.");
            
            Connection.Disconnect();
            Environment.Exit(0);
        }
        
        
        public override void SendData(byte[] data)
        {
            Connection.SendData(data);
        }

        public override ServerBase GetServer()
        {
            return null; // As it's clientside
        }

        public override ConnectionBase GetConnection()
        {
            return Connection;
        }

        public override ulong GetId()
        {
            return ClientId;
        }

        public override void Disconnect()
        {
            Connection.Disconnect();
        }

        public override Room GetRoom()
        {
            return null; // As it's clientside.
        }

        protected override void OnMovedToRoom(Room room)
        {
            
        }

        public ChatClient(string username, string ip) : base(username)
        {
            MultiNoaLoggingManager.Logger.Information("Starting Client...");
            Connection = new TcpConnection(ChatApp.ProtocolVersion);
            Connection.Connect(ip, ChatApp.ServerPort);
            Connection.SetClient(this);
            
            Thread.Sleep(2000);
            StartTyping();
        }
    }
}