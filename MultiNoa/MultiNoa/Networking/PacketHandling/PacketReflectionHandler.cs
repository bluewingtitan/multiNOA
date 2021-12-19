using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MultiNoa.Logging;
using MultiNoa.Networking.Client;
using MultiNoa.Networking.Transport;

namespace MultiNoa.Networking.PacketHandling
{
    /// <summary>
    /// Handles Packets in a Reflective Manner.
    /// </summary>
    public class PacketReflectionHandler: IPacketHandler
    {
        private static readonly ConcurrentDictionary<int, HandlerInfo> Infos = new ConcurrentDictionary<int, HandlerInfo>();


        public static void RegisterAssembly(Assembly a)
        {
            var types = from t in a.DefinedTypes
                where t.IsDefined(typeof(PacketHandler))
                select t;

            foreach (var typeInfo in types)
            {
                RegisterPacketHandler(typeInfo);
            }
        }
        
        
        public static void RegisterPacketHandler(Type t)
        {
            // Check if t is valid type.
            if (!(t.GetCustomAttribute(typeof(PacketHandler)) is PacketHandler attribute))
            {
                throw new ArgumentException($"Passed type {t.FullName} does not contain PacketClass Attribute");
            }
            

            var isInternal = (t.GetCustomAttribute(typeof(MultiNoaInternal)) is MultiNoaInternal);
            
            
            var props = t.GetMethods()
                .Where(e => e.GetCustomAttributes(typeof(HandlerMethod), true).Length > 0)
                .Select(e =>
                {
                    if (!e.IsStatic)
                    {
                        MultiNoaLoggingManager.Logger.Warning($"Method {e.Name} is declared as handler method but not static.");
                        return new KeyValuePair<MethodInfo, HandlerMethod>(null, null);
                    }
                    
                    
                    // Not getting single specific attribute, as other attributes will be implemented at this point later on too!
                    var data = e.GetCustomAttributes().ToArray();

                    for (int i = 0; i < data.Length; i++)
                        if (data[i] is HandlerMethod attr)
                        {

                            if (attr.PacketId < 0 && !isInternal)
                            {
                                throw new CustomAttributeFormatException($"Attribute {typeof(HandlerMethod).FullName} should not use packet ids under 0, as those are reserved for multiNoa internal usage!\n" +
                                                                         $"Problematic Attribute: {e.Name} at {t.FullName}");
                            }
                            
                            return new KeyValuePair<MethodInfo, HandlerMethod>(e, attr);
                        }

                    return new KeyValuePair<MethodInfo, HandlerMethod>(null, null);

                })
                .Where(e => e.Key != null && e.Value != null);

            var keyValuePairs = props.ToList();
            if(!keyValuePairs.Any())
                throw new ArgumentException($"Passed type {t.FullName} has no packet handling methods!");

            foreach (var (info, method) in keyValuePairs)
            {
                var pars = info.GetParameters();

                List<ParameterCache> pi = new List<ParameterCache>();
                foreach (var parameterInfo in pars)
                {
                    // Allowed:
                    // 1. Object
                    // 2. IConnection
                    // 3. Any Type with PacketStruct Attribute (Will try to cast)

                    if (parameterInfo.ParameterType == typeof(object))
                    {
                        pi.Add(new ParameterCache(ParamterMode.Object, parameterInfo));
                        continue;
                    }
                    
                    if (typeof(ConnectionBase).IsAssignableFrom(parameterInfo.ParameterType))
                    {
                        pi.Add(new ParameterCache(ParamterMode.Connection, parameterInfo));
                        continue;
                    }

                    if (parameterInfo.ParameterType.GetCustomAttribute(typeof(PacketStruct)) != null)
                    {
                        pi.Add(new ParameterCache(ParamterMode.PacketStruct, parameterInfo));
                        continue;
                    }
                    
                    if(parameterInfo.IsOptional)
                        continue;
                    
                    throw new ArgumentException($"Method {info.Name} in {t.FullName} has unaccepted parameter: {parameterInfo.Name}");
                    
                }
                
                Infos[method.PacketId] = new HandlerInfo(method.PacketId, info, method, pi);
            }
            if(!isInternal)
                MultiNoaLoggingManager.Logger.Debug($"Registered Handlers for type '{t.FullName}'");
        }

        public bool HandlePacket(byte[] packetBytes, ConnectionBase fromClient)
        {
            return HandlePacketStatic(packetBytes, fromClient);
        }

        public Action PrepareHandling(byte[] packetBytes, ConnectionBase fromClient)
        {
            return PrepareHandlingStatic(packetBytes, fromClient);
        }

        public static bool HandlePacketStatic(byte[] b, ConnectionBase fromClient)
        {
            PrepareHandlingStatic(b, fromClient)();
            return true;
        }

        public static Action PrepareHandlingStatic(byte[] b, ConnectionBase fromClient)
        {
            var o = PacketConverter.BytesToObject(b);
            
            var type = o.GetType();
            
            if (!(type.GetCustomAttribute(typeof(PacketStruct)) is PacketStruct attribute))
            {
                throw new PacketConversionException($"Type {type.FullName} does not contain PacketClass Attribute");
            }

            if (!Infos.ContainsKey(attribute.PacketId))
            {
                throw new PacketConversionException($"No registered Handler for PacketId #{attribute.PacketId} with type {type.FullName}");
            }
            
            var i = Infos[attribute.PacketId];


            if (i.HandlerMethod.IsSpecialHandlerAttribute)
            {
                if (i.HandlerMethod is SecuredHandlerMethod shm)
                {
                    var c = fromClient.GetClient();
                    if (c is IServersideClient sc)
                    {
                        var allowed = sc.GetAuthorityGroup((AuthorityGroup) shm.AllowedGroup);

                        if (!allowed)
                        {
                            return () => MultiNoaLoggingManager.Logger.Information($"Client {fromClient.GetEndpointIp()} tried to invoke action of type {type.FullName} / Handler {i.MethodInfo.Name}");
                        }
                    }
                }
            }
            
            
            var pars = new object[i.ParameterCaches.Count];

            for (int j = 0; j < i.ParameterCaches.Count; j++)
            {
                var parameterCache = i.ParameterCaches[j];
                switch (parameterCache.Mode)
                {
                    case ParamterMode.Object:
                        pars[j] = o;
                        break;
                    case ParamterMode.Connection:
                        pars[j] = fromClient;
                        break;
                    case ParamterMode.PacketStruct:
                        var casted = Convert.ChangeType(o, parameterCache.Info.ParameterType);
                        pars[j] = casted;
                        break;
                }
            }
            
            return () => i.MethodInfo.Invoke(null, pars);
        }
    }


    internal class HandlerInfo
    {
        internal readonly List<ParameterCache> ParameterCaches;
        internal readonly int PacketId;
        internal readonly MethodInfo MethodInfo;
        internal readonly HandlerMethod HandlerMethod;

        public HandlerInfo(int packetId, MethodInfo methodInfo, HandlerMethod handlerMethod, List<ParameterCache> parameterCaches)
        {
            PacketId = packetId;
            MethodInfo = methodInfo;
            HandlerMethod = handlerMethod;
            ParameterCaches = parameterCaches;
        }
    }


    internal class ParameterCache
    {
        internal readonly ParamterMode Mode;
        internal readonly ParameterInfo Info;

        public ParameterCache(ParamterMode mode, ParameterInfo info)
        {
            Mode = mode;
            Info = info;
        }
    }

    internal enum ParamterMode
    {
        Connection,
        Object,
        PacketStruct
    }
    
    
    [AttributeUsage(AttributeTargets.Class)]
    public class PacketHandler : Attribute
    {
        
        public PacketHandler()
        {
            
        }
    }


    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal class MultiNoaInternal : Attribute
    {
        // Empty Attribute that enables usage of packet-ids under 0 (multiNoa-management packages) for structs and handlers
    }
    
    
    [AttributeUsage(AttributeTargets.Method)]
    public class HandlerMethod : Attribute
    {
        public readonly int PacketId;
        public bool IsSpecialHandlerAttribute { get; protected set; } = false;
        public HandlerMethod(int packetId)
        {
            PacketId = packetId;
        }
    }

    public class SecuredHandlerMethod : HandlerMethod
    {
        public readonly byte AllowedGroup;
        public SecuredHandlerMethod(int packetId, byte allowedGroup) : base(packetId)
        {
            IsSpecialHandlerAttribute = true;
            if (allowedGroup > 8) allowedGroup = 8;
            AllowedGroup = allowedGroup;
        }
    }
}