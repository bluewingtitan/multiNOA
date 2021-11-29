using System;
using MultiNoa.Networking.Transport.Connection;

namespace MultiNoa.Networking.Server
{
    public class NoaTcpServer: ServerBase
    {
        public NoaTcpServer(ushort port, string protocolVersion, int tps = 5, string name = "New Server") : 
            base(port, protocolVersion, new NoaTcpListener(port, protocolVersion), tps, name) {}

        protected override void OnUpdate()
        {
            
        }

        protected override void OnPerSecondUpdate()
        {
            
        }

        protected override void OnStop()
        {
            
        }
    }
}