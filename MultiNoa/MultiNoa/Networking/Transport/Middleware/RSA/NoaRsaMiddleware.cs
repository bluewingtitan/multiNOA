using System.Collections.Generic;
using System.Security.Cryptography;
using MultiNoa.Logging;
using MultiNoa.Networking.ControlPackets;
using MultiNoa.Networking.Data.DataContainer.Generic;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Transport;
using MultiNoa.Networking.Transport.Middleware;

namespace MultiNoaCryptography.RSA
{
    [PacketHandler]
    [MultiNoaInternal]
    public class NoaRsaMiddleware: INoaMiddleware
    {
        private static NoaRsaMiddleware instance;
        
        private const int KeySize = 2048;

        public NoaRsaMiddleware()
        {
            instance = this;
        }
        
        public bool DoesModify() => true;

        public void OnConnectedServerside(ConnectionBase connection)
        {
            MultiNoaLoggingManager.Logger.Debug($"Initializing Diffie Hellman Key Exchange with connection {connection.GetEndpointIp()}");
            
            // Generate keys
            var rsa = new RSACryptoServiceProvider(KeySize);
            var key = rsa.ExportRSAPublicKey();
            
            var data = new EncryptionData(rsa, null);
            connection.SetMiddlewareData(instance, data);
            
            MultiNoaLoggingManager.Logger.Debug($"Sending {connection.GetEndpointIp()} public key of size {key.Length}");
            
            connection.SendData(new RsaKeyFromServer
            {
                Key = new NetworkArray<byte>(key)
            }, skipMiddlewares: true);
        }

        // TODO: Split up into blocks of 256 bytes!
        public List<byte> OnSend(List<byte> data, ConnectionBase connection)
        {
            if (!connection.TryGetMiddlewareData(instance, out var mData))
            {
                return data;
            }

            if (mData is EncryptionData encryptionData)
            {
                if (!encryptionData.Initialized)
                    return data;
                
                return new List<byte>(
                    encryptionData.DistantRsa.Encrypt(data.ToArray(), RSAEncryptionPadding.Pkcs1)
                );
            }
            
            MultiNoaLoggingManager.Logger.Error($"Wasn't able to encrypt message to {connection.GetEndpointIp()}\n" +
                                                $"Reason: Faulty key data.");
            return data;

        }

        public List<byte> OnReceive(List<byte> data, ConnectionBase connection)
        {
            if (!connection.TryGetMiddlewareData(instance, out var mData))
            {
                return data;
            }

            if (mData is EncryptionData encryptionData)
            {
                if (!encryptionData.Initialized)
                    return data;
                
                return new List<byte>(
                    encryptionData.LocalRsa.Decrypt(data.ToArray(), RSAEncryptionPadding.Pkcs1)
                );
            }
            
            MultiNoaLoggingManager.Logger.Error($"Wasn't able to encrypt message to {connection.GetEndpointIp()}\n" +
                                                $"Reason: Faulty key data.");
            return data;

        }
        
        private class EncryptionData
        {
            public bool Initialized = false;
            public readonly RSACryptoServiceProvider LocalRsa;
            public RSACryptoServiceProvider DistantRsa = null;

            public EncryptionData(RSACryptoServiceProvider localRsa, RSACryptoServiceProvider distantRsa)
            {
                LocalRsa = localRsa;
                DistantRsa = distantRsa;
            }
        }


        #region Packet Handling
        
        [MultiNoaInternal]
        [PacketStruct(NoaControlPacketIds.FromServer.RsaKeyFromServer)]
        public struct RsaKeyFromServer
        {
            [NetworkProperty(0)]
            public NetworkArray<byte> Key { get; set; }
        }
        
        [MultiNoaInternal]
        [PacketStruct(NoaControlPacketIds.FromClient.RsaKeyFromClient)]
        public struct RsaKeyFromClient
        {
            [NetworkProperty(0)]
            public NetworkArray<byte> Key { get; set; }
        }

        [HandlerMethod(NoaControlPacketIds.FromServer.RsaKeyFromServer)]
        public static void HandleRsaKeyFromServer(ConnectionBase connection, RsaKeyFromServer fromServer)
        {
            var distantKey = fromServer.Key.GetTypedValue();
            
            var distantRsa = new RSACryptoServiceProvider();
            distantRsa.ImportRSAPublicKey(distantKey, out var readLength);
            
            MultiNoaLoggingManager.Logger.Debug($"Initializing Diffie Hellman Key Exchange with connection {connection.GetEndpointIp()}");
            
            // Generate keys
            var rsa = new RSACryptoServiceProvider(KeySize);
            var key = rsa.ExportRSAPublicKey();

            var data = new EncryptionData(rsa, distantRsa) {Initialized = true};

            connection.SetMiddlewareData(instance, data);
            
            MultiNoaLoggingManager.Logger.Debug($"Sending {connection.GetEndpointIp()} public key of size {distantKey.Length}");
            
            connection.SendData(new RsaKeyFromClient
            {
                Key = new NetworkArray<byte>(key)
            }, skipMiddlewares: true);
        }
        
        [HandlerMethod(NoaControlPacketIds.FromClient.RsaKeyFromClient)]
        public static void HandleRsaKeyFromClient(ConnectionBase connection, RsaKeyFromClient fromClient)
        {
            var key = fromClient.Key.GetTypedValue();
            
            if (!connection.TryGetMiddlewareData(instance, out var mData))
            {
                MultiNoaLoggingManager.Logger.Error($"Wasn't able to conclude public key exchange with {connection.GetEndpointIp()}! The connection will stay unencrypted!\n" +
                                                    $"Reason: Lost private key.");
                return;
            }

            if (!(mData is EncryptionData encryptionData))
            {
                MultiNoaLoggingManager.Logger.Error($"Wasn't able to conclude public key exchange with {connection.GetEndpointIp()}! The connection will stay unencrypted!\n" +
                                                    $"Reason: Faulty key data.");
                return;
            }
            
            var newRsa = new RSACryptoServiceProvider();
            newRsa.ImportRSAPublicKey(key, out var readLength);

            encryptionData.DistantRsa = newRsa;
            encryptionData.Initialized = true;
            
            connection.SetMiddlewareData(instance, encryptionData);

        }

        #endregion
    }

    
    
}