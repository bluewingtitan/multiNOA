using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using MultiNoa.Extensions;
using MultiNoa.Logging;
using MultiNOA.Networking.Common.NetworkData;

namespace MultiNoa.Networking.PacketHandling
{
    /// <summary>
    /// A helper class that allows the full conversion of 
    /// </summary>
    public static class PacketConverter
    {
        private static readonly ConcurrentDictionary<int, PacketClassInfo> Infos = new ConcurrentDictionary<int, PacketClassInfo>();

        /// <summary>
        /// May be used on program load to have all important packet structures ready.
        /// </summary>
        public static void CachePacketStructure(Type t)
        {
            // Check if t is valid type.
            if (!(t.GetCustomAttribute(typeof(PacketClass)) is PacketClass attribute))
            {
                throw new ArgumentException("Passed type does not contain PacketClass Attribute");
            }
            
            // sorry for the mess, but mum never told me to clean up my code...
            
            var props = t.GetProperties()
                .Where(e => e.GetCustomAttributes(typeof(NetworkProperty), true).Length > 0)
                .Select(e =>
                {
                    // Check for valid property!
                    if (!e.PropertyType.GetInterfaces().Contains(typeof(INetworkDataContainer)))
                    {
                        MultiNoaLoggingManager.Logger.Warning($"Class {t.Name} contains non-serializable property: {e.Name} does not implement {nameof(INetworkDataContainer)}");
                        return new KeyValuePair<PropertyInfo, NetworkProperty>(null, null);
                    }
                    
                    // Not getting single specific attribute, as other attributes will be implemented at this point later on too!
                    var data = e.GetCustomAttributes().ToArray();

                    for (int i = 0; i < data.Length; i++)
                        if (data[i].GetType() == typeof(NetworkProperty))
                            return new KeyValuePair<PropertyInfo, NetworkProperty>(e, data[i] as NetworkProperty);

                    return new KeyValuePair<PropertyInfo, NetworkProperty>(null, null);

                })
                .Where(e => e.Key != null && e.Value != null)
                .ToList();
            
            if(props.Count <= 0)
                throw new ArgumentException("Passed type has no network serializable props!");
            
            
            // Sorting properties of class by index.
            props.Sort((pair, valuePair) =>
                (pair.Value.Index) -
                (valuePair.Value.Index)
            );
            
            Infos[attribute.PacketId] = new PacketClassInfo(new Dictionary<PropertyInfo, NetworkProperty>(props));
        }


        public static byte[] ObjectToByte(object o)
        {
            var bytes = new List<byte>();
            
            var type = o.GetType();

            if (!(type.GetCustomAttribute(typeof(PacketClass)) is PacketClass attribute))
            {
                throw new ArgumentException("Passed type does not contain PacketClass Attribute");
            }


            if (!Infos.ContainsKey(attribute.PacketId))
            {
                throw new Exception("Uncached packet!");
                CachePacketStructure(type);
            }

            var data = Infos[attribute.PacketId];


            foreach (var (prop, attributeData) in data.Props)
            {
                // Already checked that prop implements INetworkDataContainer!
                if (prop.GetValue(o) is INetworkDataContainer c)
                {
                    bytes.AddRange(c.TurnIntoBytes());
                }
                else
                {
                    MultiNoaLoggingManager.Logger.Warning(
                        $"Failed to convert property {prop.Name} to bytes: Prop was null");
                }
            }

            return bytes.ToArray();
        }

        public static T BytesToObject<T>(byte[] b)
        {
            var type = typeof(T);
            if (!(type.GetCustomAttribute(typeof(PacketClass)) is PacketClass attribute))
            {
                throw new ArgumentException("Passed type does not contain PacketClass Attribute");
            }
            
            object instance = Activator.CreateInstance<T>();


            if (!Infos.ContainsKey(attribute.PacketId))
            {
                CachePacketStructure(type);
            }

            var data = Infos[attribute.PacketId];


            foreach (var (prop, attributeData) in data.Props)
            {
                var container = Activator.CreateInstance(prop.PropertyType) as INetworkDataContainer;
                
                var l = container.LoadFromBytes(b);
                b = b.GetSubarray(l, b.Length - l);
                    
                prop.SetValue(instance, container);
            }

            return (T) instance;
        }
        
        
    }

    internal class PacketClassInfo
    {
        internal readonly Dictionary<PropertyInfo, NetworkProperty> Props;

        public PacketClassInfo(Dictionary<PropertyInfo, NetworkProperty> props)
        {
            Props = props;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class NetworkProperty : Attribute
    {
        public readonly int Index;
        
        /// <summary>
        /// Grants extra information to the Packet Converter, including index (missing indices will be skipped) of the Property.
        /// </summary>
        /// <param name="index">Force specific index of field in/from (de-)serialized packet</param>
        public NetworkProperty(int index = 0)
        {
            Index = index;
        }
    }

    [AttributeUsage(AttributeTargets.Struct)]
    public class PacketClass : Attribute
    {
        public readonly int PacketId;
        public PacketClass(int packetId)
        {
            PacketId = packetId;
        }
    }
    
}