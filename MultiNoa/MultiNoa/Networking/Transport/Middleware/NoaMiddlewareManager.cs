using System.Collections.Generic;

namespace MultiNoa.Networking.Transport.Middleware
{
    public static class NoaMiddlewareManager
    {

        public static void OnConnectedServerside(ConnectionBase connection)
        {
            foreach (var nonModifyingMiddleware in MultiNoaSetup.NonModifyingMiddlewares)
            {
                nonModifyingMiddleware.OnConnectedServerside(connection);
            }
            
            foreach (var nonModifyingMiddleware in MultiNoaSetup.ModifyingMiddlewares)
            {
                nonModifyingMiddleware.OnConnectedServerside(connection);
            }
        }
        
        public static List<byte> OnSend(List<byte> bytes, ConnectionBase connection)
        {
            foreach (var nonModifyingMiddleware in MultiNoaSetup.NonModifyingMiddlewares)
            {
                nonModifyingMiddleware.OnSend(bytes, connection);
            }

            var nextBytes = bytes;
            
            for (var i = 0; i < MultiNoaSetup.ModifyingMiddlewares.Length; i++)
            {
                nextBytes = MultiNoaSetup.ModifyingMiddlewares[i].OnSend(nextBytes, connection);
            }

            return nextBytes;
        }

        public static List<byte> OnReceive(List<byte> bytes, ConnectionBase connection)
        {
            foreach (var nonModifyingMiddleware in MultiNoaSetup.NonModifyingMiddlewares)
            {
                nonModifyingMiddleware.OnReceive(bytes, connection);
            }

            var nextBytes = bytes;
            
            // Iterating in reverse order this time
            for (var i = MultiNoaSetup.ModifyingMiddlewares.Length - 1; i >= 0; i--)
            {
                nextBytes = MultiNoaSetup.ModifyingMiddlewares[i].OnReceive(nextBytes, connection);
            }

            return nextBytes;
        }
        
    }
}