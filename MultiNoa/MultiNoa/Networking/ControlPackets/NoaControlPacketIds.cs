using MultiNoa.Networking.Transport.Middleware.Fragmentation;

namespace MultiNoa.Networking.ControlPackets
{
    /// <summary>
    /// A collection of packet ids for packets used inside of multiNoa to transfer information
    /// </summary>
    internal static class NoaControlPacketIds
    {
        public static class FromServer
        {
            private const int Offset = -1024;
            public const int ConnectionRefusedPacket = Offset + 0;
            public const int WelcomePacket = Offset + 1;

            #region Encryption

            public const int RsaKeyFromServer = Offset + 64;

            #endregion
        }

        public static class FromClient
        {
            private const int Offset = -2048;
            public const int WelcomeReceived = Offset + 0;
            public const int SyncUsername = Offset + 1;
            
            #region Encryption

            public const int RsaKeyFromClient = Offset + 64;
            
            #endregion
        }


        public static class Symmetrical
        {
            private const int Offset = -4069;
            
            #region Fragmentation

            public const int FragmentPacket = Offset + 0;

            #endregion
            
        }
        
        
    }
}