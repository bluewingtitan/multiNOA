using System;
using System.Collections.Generic;
using System.Reflection;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Transport.Middleware;

namespace MultiNoa
{
    /// <summary>
    /// The main class, offering control over most general things and abstractions.
    /// </summary>
    public static class MultiNoaSetup
    {
        internal static DynamicThread DefaultThread = new DynamicThread(2, "MultiNoa Default");
        
        public const string VersionCode = "alpha-1.0";
        internal static bool SetupDone = false;
        internal static INoaMiddleware[] NonModifyingMiddlewares = new INoaMiddleware[0];
        internal static INoaMiddleware[] ModifyingMiddlewares = new INoaMiddleware[0];
        
        
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
                    ExtraAssemblies = new Assembly[0],
                    Middlewares = new INoaMiddleware[] {new NoaNetworkLoggingMiddleware()}
                });
        }

        /// <summary>
        /// Only use if you need a setup that definitely differs from the default setup!
        /// </summary>
        /// <param name="config">Configuration Class Instance</param>
        public static void CustomSetup(MultiNoaConfig config)
        {
            Setup(config);
        }


        private static void Setup(MultiNoaConfig config)
        {
            if (SetupDone)
            {
                MultiNoaLoggingManager.Logger.Warning($"Tried to setup multiNoa a second time. Stack Trace: {Environment.StackTrace}");
            }
            SetupDone = true;

            PacketReflectionHandler.RegisterAssembly(typeof(MultiNoaConfig).Assembly);
            PacketReflectionHandler.RegisterAssembly(config.MainAssembly);
            foreach (var configExtraAssembly in config.ExtraAssemblies)
                PacketReflectionHandler.RegisterAssembly(configExtraAssembly);
            
            
            
            PacketConverter.RegisterAssembly(typeof(MultiNoaConfig).Assembly);
            PacketConverter.RegisterAssembly(config.MainAssembly);
            DataContainerManager.RegisterAssembly(config.MainAssembly);
            DataContainerManager.RegisterAssembly(typeof(MultiNoaConfig).Assembly);
            foreach (var configExtraAssembly in config.ExtraAssemblies)
            {
                PacketConverter.RegisterAssembly(configExtraAssembly);
                DataContainerManager.RegisterAssembly(configExtraAssembly);
            }
        }


        private static void RegisterMiddlewares(INoaMiddleware[] middlewares)
        {
            var modifying = new List<INoaMiddleware>();
            var notModifying = new List<INoaMiddleware>();

            foreach (var middleware in middlewares)
            {
                if (middleware.DoesModify())
                {
                    modifying.Add(middleware);
                }
                else
                {
                    notModifying.Add(middleware);
                }
            }

            NonModifyingMiddlewares = notModifying.ToArray();
            ModifyingMiddlewares = modifying.ToArray();
        }
    }

    public struct MultiNoaConfig
    {
        public INoaMiddleware[] Middlewares;
        public Assembly[] ExtraAssemblies;
        public Assembly MainAssembly;

        public MultiNoaConfig(Assembly mainAssembly, Assembly[] extraAssemblies, INoaMiddleware[] middlewares)
        {
            MainAssembly = mainAssembly;
            ExtraAssemblies = extraAssemblies;
            Middlewares = middlewares;
        }
    }

}