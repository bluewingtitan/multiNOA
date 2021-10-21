﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MultiNoa.Extensions;
using MultiNoa.Logging;
using MultiNOA.Networking.Common.NetworkData;
using MultiNOA.Networking.Common.NetworkData.DataContainer;

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
                throw new ArgumentException($"Passed type {t.Name} does not contain PacketClass Attribute");
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
                throw new ArgumentException($"Passed type {t.Name} has no network serializable props!");
            
            
            // Sorting properties of class by index.
            props.Sort((pair, valuePair) =>
                (pair.Value.Index) -
                (valuePair.Value.Index)
            );
            
            Infos[attribute.PacketId] = new PacketClassInfo(t, new Dictionary<PropertyInfo, NetworkProperty>(props), attribute);
        }


        public static byte[] ObjectToByte(object o, bool skipTypeCheck = false)
        {
            var bytes = new List<byte>();
            
            var type = o.GetType();

            if (!(type.GetCustomAttribute(typeof(PacketClass)) is PacketClass attribute))
            {
                throw new PacketConversionException($"Passed type {type.Name} does not contain PacketClass Attribute");
            }


            if (!Infos.ContainsKey(attribute.PacketId))
            {
                CachePacketStructure(type);
            }
            
            var data = Infos[attribute.PacketId];

            if (!skipTypeCheck && data.Type != type)
                throw new PacketConversionException($"ID is not unique: {type.Name} tried to use #{attribute.PacketId} assigned to {data.Type.Name}");


            // write packet id into byte-array
            bytes.AddRange(new NetworkInt(attribute.PacketId).TurnIntoBytes());
            



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

        public static object BytesToObject(byte[] b)
        {
            var idContainer = new NetworkInt();
            idContainer.LoadFromBytes(b);
            b = b.GetSubarray(4, b.Length - 4);
            var pId = idContainer.GetValue();

            if (!Infos.ContainsKey(pId))
            {
                throw new PacketConversionException($"No Packet Type known with Id #{pId}");
            }
            
            
            var data = Infos[pId];

            var type = data.Type;
            
            if (!(type.GetCustomAttribute(typeof(PacketClass)) is PacketClass attribute))
            {
                throw new PacketConversionException($"Type {type} does not contain PacketClass Attribute");
            }
            
            var instance = Activator.CreateInstance(type);



            foreach (var (prop, attributeData) in data.Props)
            {
                var container = Activator.CreateInstance(prop.PropertyType) as INetworkDataContainer;
                
                var l = container.LoadFromBytes(b);
                b = b.GetSubarray(l, b.Length - l);
                    
                prop.SetValue(instance, container);
            }

            return instance;
        }
        
        
        public static T BytesToObject<T>(byte[] b, bool containsPacketId = true, bool skipTypeCheck = false)
        {
            var type = typeof(T);
            if (!(type.GetCustomAttribute(typeof(PacketClass)) is PacketClass attribute))
            {
                throw new PacketConversionException($"Passed type {type} does not contain PacketClass Attribute");
            }
            
            object instance = Activator.CreateInstance<T>();


            if (!Infos.ContainsKey(attribute.PacketId))
            {
                CachePacketStructure(type);
            }

            var data = Infos[attribute.PacketId];
            
            
            if (containsPacketId)
            {
                var idContainer = new NetworkInt();
                idContainer.LoadFromBytes(b);
                b = b.GetSubarray(4, b.Length - 4);

                var pId = idContainer.GetValue();

                if (!skipTypeCheck && pId != attribute.PacketId)
                    throw new PacketConversionException($"Tried to parse packet with type-id{pId} to {type.Name}, should be #{attribute.PacketId}");
            }
            



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


    public class PacketConversionException : Exception
    {
        public PacketConversionException(string msg) : base(msg){}
    }

    internal class PacketClassInfo
    {
        internal readonly Type Type;
        internal readonly PacketClass PacketClass;
        internal readonly Dictionary<PropertyInfo, NetworkProperty> Props;

        public PacketClassInfo(Type type, Dictionary<PropertyInfo, NetworkProperty> props, PacketClass packetClass)
        {
            Props = props;
            PacketClass = packetClass;
            Type = type;
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
        public readonly Type Handler;
        public PacketClass(int packetId, Type handler = null)
        {
            PacketId = packetId;
            Handler = handler;
        }
    }
    
}