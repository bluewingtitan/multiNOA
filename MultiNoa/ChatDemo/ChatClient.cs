using System;
using System.Threading;
using MultiNoa.Logging;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Transport;
using MultiNoa.Networking.Transport.Connection;

namespace ChatDemo
{
    public class ChatClient: ClientBase, IUserSideClient
    {
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
                SendData(msg);
            }
            
            Console.WriteLine("Thank you for using NoaChat.");
            
            Connection.Disconnect();
            Environment.Exit(0);
        }
        
        
        public override void SendData(object data)
        {
            Connection.SendData(data);
        }


        public override ConnectionBase GetConnection()
        {
            return Connection;
        }

        public override void Disconnect()
        {
            Connection.Disconnect();
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