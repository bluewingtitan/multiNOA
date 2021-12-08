using System;
using System.Threading;
using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Data.DataContainer;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Transport.Middleware;

namespace MultiNoa.Networking.Transport
{
    public abstract class ConnectionBase: IUpdatable
    {

        protected ConnectionBase()
        {
            ChangeThread(MultiNoaSetup.DefaultThread);
            
            // Prepare for horror.
            OnDisconnected += c => c.GetClient()?.GetRoom().RemoveClient(c.GetClient());
            // Recover from horror.
        }
        
        protected const int DataBufferSize = 4096;

        public delegate void ConnectionEventDelegate(ConnectionBase connection);

        public event ConnectionEventDelegate OnConnected;
        public event ConnectionEventDelegate OnDisconnected;

        public abstract void Update();
        public abstract void PerSecondUpdate();
        public abstract void SetPacketHandler(IPacketHandler newHandler);
        public abstract string GetEndpointIp();
        protected abstract void TransferData(byte[] data);
        public abstract ClientBase GetClient();
        public abstract void SetClient(ClientBase client);


        public void SendData(object objectToSend, bool stayInThread = false)
        {
            if (stayInThread)
            {
                SendDataInThread(objectToSend);
                return;
            }

            // => Do struct to packet, middleware and other logic in seperate thread to keep main game threads running
            var t = new Thread(() =>
            {
                var bytes = PacketConverter.ObjectToByte(objectToSend, writeLength: false);

                bytes = NoaMiddlewareManager.OnSend(bytes, this);

                // Insert length
                bytes.InsertRange(0, new NetworkInt(bytes.Count).TurnIntoBytes());

                TransferData(bytes.ToArray());
            });
            t.Start();
        }


        private void SendDataInThread(object objectToSend)
        {
            var bytes = PacketConverter.ObjectToByte(objectToSend, writeLength: false);

            bytes = NoaMiddlewareManager.OnSend(bytes, this);
            
            // Insert length
            bytes.InsertRange(0, new NetworkInt(bytes.Count).TurnIntoBytes());

            TransferData(bytes.ToArray());
        }
        
        
        
        internal void InvokeOnConnected()
        {
            OnConnected?.Invoke(this);
        }
        
        public void Disconnect()
        {
            OnDisconnected?.Invoke(this);
            OnDisconnect();
        }
        
        protected abstract void OnDisconnect();
        public abstract void ChangeThread(IDynamicThread newThread);
        public abstract string GetProtocolVersion();
    }
}