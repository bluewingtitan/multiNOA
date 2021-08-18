using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MultiNOA.Attributes.PacketHandling;
using MultiNOA.Middleware;
using MultiNOA.Networking.Common;

namespace MultiNOA.Networking.PacketHandling
{
    /// <summary>
    /// Dynamically sits between the network layer and your PacketHandlers. Acts on a specific enum type.
    /// </summary>
    /// <typeparam name="T1">Enum Type</typeparam>
    public class PacketHandlingAgent<T1> where T1 : Enum
    {
        private readonly Dictionary<T1, MethodInfo> _methodInfos;
        private readonly bool _staticMode;
        private readonly Type _handlerType;
        private readonly IPacketHandler _handlerInstance;

        
        public PacketHandlingResult HandlePacket(Packet packet)
        {
            if (!packet.Finalized)
            {
                MultiNoaLoggingManager.Logger.Warning("Tried to handle unfinalized packet");
            }
            
            // 1. Read Packet Type
            
            // 2. Check if Packet Type is in handler list

            if (true)
            {
                // Packet can't be handled if no handler is defined.
                return new PacketHandlingResult(false);
            }
            
            // 3. Handle


            return new PacketHandlingResult(true);

        }
        
        
        /// <summary>
        /// Creates a Packet Handling Agent that uses an Instance of a handler (use for non-static methods)
        /// </summary>
        /// <param name="handlerInstance">Instance of class containing handler methods</param>
        public PacketHandlingAgent(IPacketHandler handlerInstance)
        {
            _staticMode = false;
            _methodInfos = GetHandlerMethods(handlerInstance.GetType(), false);
            _handlerInstance = handlerInstance;
            _handlerType = handlerInstance.GetType();
        }

        
        
        /// <summary>
        /// Creates a Packet Handling Agent that uses static methods of a Type
        /// </summary>
        /// <param name="handlerType">Type that contains static methods</param>
        public PacketHandlingAgent(Type handlerType)
        {
            _handlerType = handlerType;
            _staticMode = true;
            _methodInfos = GetHandlerMethods(handlerType, true);
        }

        /// <summary>
        /// Generates a Packet Handling Agent that uses all static packet handlers inside a defined assembly, that react to the set enum-type.
        /// </summary>
        /// <param name="assembly"></param>
        public PacketHandlingAgent(Assembly assembly)
        {
            
        }



        private static Dictionary<T1, MethodInfo> GetHandlerMethods(Type t, bool staticMethods)
        {
            var r = new Dictionary<T1, MethodInfo>();
            var methods = t.GetMethods()
                .Where(m => m.GetCustomAttributes(typeof(PacketHandlerAttribute), false).Length > 0)
                .Where(m => m.IsStatic == staticMethods)
                .ToArray();
            
            // Analyze preselected methods
            foreach (var method in methods)
            {
                if (!(method.GetCustomAttribute(typeof(PacketHandlerAttribute)) is PacketHandlerAttribute attribute)) continue;
                if(attribute.ActOn.GetType() == typeof(T1)) continue;
                
                r.Add((T1) attribute.ActOn, method);
            }
            
            
            return r;
        }


    }

    public interface IPacketHandler
    {
        
    }
    
}