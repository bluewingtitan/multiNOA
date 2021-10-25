using System;
using System.Diagnostics;
using System.Reflection;
using MultiNoa.Logging;
using MultiNoa.Networking.PacketHandling;

namespace MultiNoa
{
    /// <summary>
    /// The main class, offering control over most general things and abstractions.
    /// </summary>
    public static class MultiNoaSetup
    {
        public const string VersionCode = "alpha-0.1";
        internal static bool _setupDone = false;
        
        /// <summary>
        /// Sets up MultiNoa in a way fitting for most use cases, given that all dynamically handled packet types are defined within the given assembly.
        /// </summary>
        /// <param name="mainAssembly"></param>
        public static void DefaultSetup(Assembly mainAssembly)
        {
            Setup(
                new MultiNoaConfig
                {
                    MainAssembly = mainAssembly,
                    ConversionMode = PacketConversionMode.Reflective,
                    HandlingMode = PacketHandlingMode.Reflective
                });
        }


        private static void Setup(MultiNoaConfig config)
        {
            if (_setupDone)
            {
                MultiNoaLoggingManager.Logger.Warning($"Tried to setup multiNoa a second time. Stack Trace: {Environment.StackTrace}");
            }
            _setupDone = true;

            switch (config.HandlingMode)
            {
                case PacketHandlingMode.Reflective:
                    PacketReflectionHandler.RegisterAssembly(config.MainAssembly);
                    break;
                
                case PacketHandlingMode.Custom:
                default:
                    break;
            }
            
            switch (config.ConversionMode)
            {
                case PacketConversionMode.Reflective:
                    PacketConverter.RegisterAssembly(config.MainAssembly);
                    break;
                
                case PacketConversionMode.Custom:
                default:
                    break;
            }
            

        }
    }



    public struct MultiNoaConfig
    {
        public Assembly[] ExtraAssemblies;
        public Assembly MainAssembly;
        public PacketConversionMode ConversionMode;
        public PacketHandlingMode HandlingMode;

        public MultiNoaConfig(Assembly mainAssembly, PacketConversionMode conversionMode, PacketHandlingMode handlingMode, Assembly[] extraAssemblies)
        {
            MainAssembly = mainAssembly;
            ConversionMode = conversionMode;
            HandlingMode = handlingMode;
            ExtraAssemblies = extraAssemblies;
        }
    }


    // I know that PacketConversionMode and PacketHandlingMode currently offer the exact same API, but this might not be the case forever.

    public enum PacketConversionMode
    {
        /// <summary>
        /// Set this if you intend to use the PacketConverter as Packet Converter
        /// </summary>
        Reflective,
        
        /// <summary>
        /// Set this if you intend to use your custom procedures for converting byte-arrays to data and vice-versa.
        /// </summary>
        Custom,
    }

    public enum PacketHandlingMode
    {
        /// <summary>
        /// Set this if you intend to use the PacketReflectionHandler as Packet Handler
        /// </summary>
        Reflective,
        
        /// <summary>
        /// Set this if you intend in only using your own custom packet handlers.
        /// This will skip the analysis of your assembly.
        /// </summary>
        Custom,
    }
    
    
    
    
    
    
}