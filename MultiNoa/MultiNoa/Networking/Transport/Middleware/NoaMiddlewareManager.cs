using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using MultiNoa.Logging;

namespace MultiNoa.Networking.Transport.Middleware
{
    public static class NoaMiddlewareManager
    {
        private static readonly List<byte> DummyList = new List<byte>(new byte[1]);
        
        public static void OnConnectedServerside(ConnectionBase connection)
        {
            for (var i = 0; i < CallOrder.Length; i++)
            {
                Calls[CallOrder[i]](DummyList, connection, OnConnectedServersideCall);
            }
        }
        
        public static List<byte> OnSend(List<byte> bytes, ConnectionBase connection, MiddlewareTarget[] excludes = null)
        {
            excludes ??= new MiddlewareTarget[0];
            
            for (var i = 0; i < CallOrder.Length; i++)
            {
                var target = CallOrder[i];
                if(excludes.Contains(target)) continue;
                
                bytes = Calls[target](bytes, connection, OnSendCall);
            }

            return bytes;
        }

        public static List<byte> OnReceive(List<byte> bytes, ConnectionBase connection, MiddlewareTarget[] excludes = null)
        {
            excludes ??= new MiddlewareTarget[0];
            
            for (var i = CallOrder.Length - 1; i >= 0 ; i--)
            {
                var target = CallOrder[i];
                if(excludes.Contains(target)) continue;

                bytes = Calls[target](bytes, connection, OnReceiveCall);
            }

            return bytes;
        }

        private delegate List<byte> MiddlewareCall(INoaMiddleware m, List<byte> b, ConnectionBase c);

        private delegate List<byte> CallAllMiddlewaresDelegate(List<byte> b, ConnectionBase c, MiddlewareCall call);

        private static readonly MiddlewareCall OnSendCall = (m, b, c) => m.OnSend(b, c);
        private static readonly MiddlewareCall OnReceiveCall = (m, b, c) => m.OnReceive(b, c);

        private static readonly MiddlewareCall OnConnectedServersideCall = (m, b, c) =>
        {
            m.OnConnectedServerside(c);
            return b;
        };

        private static readonly MiddlewareTarget[] CallOrder = new[]
        {
            MiddlewareTarget.Checking, MiddlewareTarget.Fragmenting, MiddlewareTarget.Encrypting,
            MiddlewareTarget.Correcting, MiddlewareTarget.NonModifying
        };
        
        
        private static readonly Dictionary<MiddlewareTarget, CallAllMiddlewaresDelegate> Calls = new Dictionary<MiddlewareTarget, CallAllMiddlewaresDelegate>()
        {
            {MiddlewareTarget.Checking, CallChecking},
            {MiddlewareTarget.Fragmenting, CallFragmenting},
            {MiddlewareTarget.Encrypting, CallEncrypting},
            {MiddlewareTarget.Correcting, CallCorrecting},
            {MiddlewareTarget.NonModifying, CallNonModifying},
        };


        private static List<byte> CallMiddlewares(INoaMiddleware[] middlewares, List<byte> b, ConnectionBase c,
            MiddlewareCall call)
        {
            if (b.Count == 0)
                return b;
            
            foreach (var middleware in middlewares)
            {
                b = call(middleware, b, c);
                
                if (b.Count == 0)
                    return b;
            }

            return b;
        }

        private static List<byte> CallChecking(List<byte> b, ConnectionBase c, MiddlewareCall call)
        {
            MultiNoaLoggingManager.Logger.Debug($"Middleware-Stack: {nameof(CallChecking)}");
            return CallMiddlewares(MultiNoaSetup.CheckingMiddlewares, b, c, call);
        }
        
        private static List<byte> CallEncrypting(List<byte> b, ConnectionBase c, MiddlewareCall call)
        {
            MultiNoaLoggingManager.Logger.Debug($"Middleware-Stack: {nameof(CallEncrypting)}");
            return CallMiddlewares(MultiNoaSetup.EncryptingMiddlewares, b, c, call);
        }
        
        
        private static List<byte> CallFragmenting(List<byte> b, ConnectionBase c, MiddlewareCall call)
        {
            MultiNoaLoggingManager.Logger.Debug($"Middleware-Stack: {nameof(CallFragmenting)}");
            return CallMiddlewares(MultiNoaSetup.FragmentingMiddlewares, b, c, call);
        }
        
        
        private static List<byte> CallCorrecting(List<byte> b, ConnectionBase c, MiddlewareCall call)
        {
            MultiNoaLoggingManager.Logger.Debug($"Middleware-Stack: {nameof(CallCorrecting)}");
            return CallMiddlewares(MultiNoaSetup.CorrectingMiddlewares, b, c, call);
        }
        
        
        private static List<byte> CallNonModifying(List<byte> b, ConnectionBase c, MiddlewareCall call)
        {
            MultiNoaLoggingManager.Logger.Debug($"Middleware-Stack: {nameof(CallNonModifying)}");
            return CallMiddlewares(MultiNoaSetup.NonModifyingMiddlewares, b, c, call);
        }

    }
}