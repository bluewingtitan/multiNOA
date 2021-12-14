using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using MultiNoa.Logging;
using MultiNoa.Networking.ControlPackets;
using MultiNoa.Networking.Data.DataContainer.Generic;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa.Networking.Transport.Middleware
{
    [PacketHandler]
    [MultiNoaInternal]
    public class NoaRsaMiddleware: INoaMiddleware
    {
        /// <summary>
        /// Used as key in the clients middleware data storage.
        /// </summary>
        private static NoaRsaMiddleware _instance = null;
        
        /// <summary>
        /// The padding to use for cryptography.
        /// </summary>
        private static readonly RSAEncryptionPadding Padding = RSAEncryptionPadding.Pkcs1;
        
        /// <summary>
        /// Amount of bytes in key size.
        /// </summary>
        private const int KeySize = 2048;
        
        /// <summary>
        /// Amount of bytes fitting into a key of specified size.
        /// </summary>
        private const int BlockSpace = 214;
        
        
        /// <summary>
        /// Amount of bytes in a block when decrypting
        /// </summary>
        private const int BlockSize = 256;

        private delegate byte[] CryptoFunction(byte[] data, RSAEncryptionPadding padding);

        public NoaRsaMiddleware()
        {
            _instance ??= this;
        }
        
        public bool DoesModify() => true;
        public void Setup()
        {
            MultiNoaLoggingManager.Logger.Information("Initialized NoaRsaMiddleware. Please be aware of the fact, that this middleware bloats up data to a multiple of 256 bytes and may be quite memory-intensive.");
        }

        public void OnConnectedServerside(ConnectionBase connection)
        {
            MultiNoaLoggingManager.Logger.Debug($"Initializing Diffie Hellman Key Exchange with connection {connection.GetEndpointIp()}");
            
            // Generate keys
            var rsa = new RSACryptoServiceProvider(KeySize);
            var key = rsa.ExportRSAPublicKey();
            
            var data = new EncryptionData(rsa, null);
            connection.SetMiddlewareData(_instance, data);
            
            MultiNoaLoggingManager.Logger.Debug($"Sending {connection.GetEndpointIp()} public key of size {key.Length}");
            
            connection.SendData(new RsaKeyFromServer
            {
                Key = new NetworkArray<byte>(key)
            }, skipMiddlewares: true);
        }

        
        public List<byte> OnSend(List<byte> data, ConnectionBase connection)
        {
            if (!connection.TryGetMiddlewareData(_instance, out var mData))
            {
                return data;
            }

            if (mData is EncryptionData encryptionData)
            {
                if (!encryptionData.Initialized)
                    return data;
                
                return CryptographyHelper(data, BlockSpace, (bytes, padding) => encryptionData.DistantRsa.Encrypt(bytes, padding));
            }
            
            MultiNoaLoggingManager.Logger.Error($"Wasn't able to encrypt message to {connection.GetEndpointIp()}\n" +
                                                $"Reason: Faulty key data.");
            return data;
        }

        private static List<byte> CryptographyHelper(List<byte> data, int blockSize, CryptoFunction cryptoFunction)
        {
            var blocks = (int) Math.Ceiling((double) data.Count / blockSize);

            var totalLength = blocks * blockSize;

            while (data.Count < totalLength)
                data.Add(0);

            for (int bn = 0; bn < blocks; bn++)
            {
                var removed = data.GetRange(0, blockSize).ToArray();
                data.RemoveRange(0, blockSize);
                
                var encrypted = cryptoFunction(removed, Padding);
                data.AddRange(encrypted);
            }

            return data;
        }
        
        
        public List<byte> OnReceive(List<byte> data, ConnectionBase connection)
        {
            if (!connection.TryGetMiddlewareData(_instance, out var mData))
            {
                return data;
            }

            if (mData is EncryptionData encryptionData)
            {
                if (!encryptionData.Initialized)
                    return data;
                
                return CryptographyHelper(data, BlockSize,(bytes, padding) => encryptionData.LocalRsa.Decrypt(bytes, padding));
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

            connection.SetMiddlewareData(_instance, data);
            
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
            
            if (!connection.TryGetMiddlewareData(_instance, out var mData))
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
            
            connection.SetMiddlewareData(_instance, encryptionData);

        }

        #endregion
    }

    
    
}