using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using MultiNoa.Extensions;
using MultiNoa.GameSimulation;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Data.DataContainer;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Transport.Middleware;

namespace MultiNoa.Networking.Transport
{
    public abstract class ConnectionBase: IUpdatable
    {
        private static readonly IPacketHandler DefaultHandler = new PacketReflectionHandler();
        private readonly ConcurrentDictionary<INoaMiddleware, object> _middlewareData = new ConcurrentDictionary<INoaMiddleware, object>();
        

        protected ConnectionBase(string protocolVersion)
        {
            _client = null;
            _handler = DefaultHandler;
            _protocolVersion = protocolVersion;
            ChangeThread(MultiNoaSetup.DefaultThread);
            
            // Prepare for horror.

            OnDisconnected += c =>
            {
                if (c.GetClient() is IServersideClient sClient)
                {

                    sClient.GetRoom().RemoveClient(sClient);
                }
            };

            // Recover from horror.
        }

        public bool TryGetMiddlewareData(INoaMiddleware middleware, out object data)
        {
            return _middlewareData.TryGetValue(middleware, out data);
        }

        public void SetMiddlewareData(INoaMiddleware middleware, object data)
        {
            _middlewareData[middleware] = data;
        }

        
        
        private IPacketHandler _handler;
        private IDynamicThread _currentThread;
        private readonly string _protocolVersion;
        
        protected const int DataBufferSize = 4096;

        private IClient _client;
        public delegate void ConnectionEventDelegate(ConnectionBase connection);

        public event ConnectionEventDelegate OnConnected;
        public event ConnectionEventDelegate OnDisconnected;

        public void Update()
        {
            _handlers.ExecuteAll();
        }
        public abstract void PerSecondUpdate();
        public abstract string GetEndpointIp();
        protected abstract void TransferData(byte[] data);
        public IClient GetClient() => _client;
        private readonly ExecutionScheduler _handlers = new ExecutionScheduler();

        public void SetClient(IClient client)
        {
            _client = client;
        }


        public void SendData(object objectToSend, bool stayInThread = false, bool skipMiddlewares = false)
        {
            if (stayInThread)
            {
                SendDataInThread(objectToSend, skipMiddlewares);
                return;
            }

            // => Do struct to packet, middleware and other logic in separate thread to keep main game threads running
            var t = new Thread(() => SendDataInThread(objectToSend, skipMiddlewares));
            t.Start();
        }


        private void SendDataInThread(object objectToSend, bool skipMiddlewares)
        {
            var bytes = PacketConverter.ObjectToByte(objectToSend, writeLength: false);

            if(!skipMiddlewares)
                bytes = NoaMiddlewareManager.OnSend(bytes, this);
            
            // Insert length
            bytes.InsertRange(0, new NetworkInt(bytes.Count).TurnIntoBytes());

            TransferData(bytes.ToArray());
        }
        
        
        
        /// <summary>
        /// Handles a byte-array containing one or multiple individual packets
        /// </summary>
        /// <param name="data"></param>
        protected void HandleData(byte[] data)
        {
            var packetLenght = 0;
            var packet = new Packet(data);

            if (packet.UnreadLength() >= 4)
            {
                packetLenght = packet.Read<NetworkInt>().GetTypedValue();
                if (packetLenght <= 0)
                {
                    return;
                }
            }
            
            while (packetLenght > 0 && packetLenght <= packet.UnreadLength())
            {
                //MultiNoaLoggingManager.Logger.Debug($"Parsing packet of size {packetLenght}");
                
                // Do packet analysis now and prepare/schedule handling for next tick
                var packetBytes = new List<byte>(packet.ReadBytes(packetLenght));

                packetBytes = NoaMiddlewareManager.OnReceive(packetBytes, this);
                
                _handlers.ScheduleExecution(_handler.PrepareHandling(packetBytes.ToArray(), this));
                
                // Analyze next packet contained in bytes
                packetLenght = 0;
                if (packet.UnreadLength() >= 4)
                {
                    packetLenght = packet.Read<NetworkInt>().GetTypedValue();
                    if (packetLenght <= 0)
                    {
                        return;
                    }
                }

                if (packetLenght <= 1)
                {
                    return;
                }
            }
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
        
        
        public void ChangeThread(IDynamicThread newThread)
        {
            if (_currentThread != null)
            {
                _currentThread.RemoveUpdatable(this); 
                _currentThread = newThread;
            }
            newThread.AddUpdatable(this);
        }
        

        public string GetProtocolVersion()
        {
            return _protocolVersion;
        }
        
        public void SetPacketHandler(IPacketHandler newHandler)
        {
            _handler = newHandler;
        }
    }
}