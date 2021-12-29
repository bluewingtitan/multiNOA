using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using MultiNoa.GameSimulation;
using MultiNoa.Logging;
using MultiNoa.Networking.PacketHandling;
using MultiNoa.Networking.Transport.Middleware;
using MultiNoa.Networking.Transport.Middleware.Fragmentation;

namespace MultiNoa
{
    /// <summary>
    /// The main class, offering control over most general things and abstractions.
    /// </summary>
    public static class MultiNoaSetup
    {
        public static int DataBufferSize { get; private set; }  = 8196;


        internal static DynamicThread DefaultThread = new DynamicThread(2, "MultiNoa Default");
        
        public const string VersionCode = "0.1.0";
        private static bool _setupDone = false;
        internal static INoaMiddleware[] CheckingMiddlewares = new INoaMiddleware[0];
        internal static INoaMiddleware[] EncryptingMiddlewares = new INoaMiddleware[0];
        internal static INoaMiddleware[] FragmentingMiddlewares = new INoaMiddleware[0];
        internal static INoaMiddleware[] CorrectingMiddlewares = new INoaMiddleware[0];
        internal static INoaMiddleware[] NonModifyingMiddlewares = new INoaMiddleware[0];

        public static readonly MultiNoaSetupCollection SetupCollection = new MultiNoaSetupCollection();
        
        
        /// <summary>
        /// Sets up MultiNoa in a way fitting for most use cases, given that all dynamically handled packet types are defined within the given assembly.
        /// </summary>
        /// <param name="mainAssembly"></param>
        internal static void DefaultSetup(Assembly mainAssembly)
        {
            Setup(
                new MultiNoaConfig
                {
                    DataBufferSize = DataBufferSize,
                    MainAssembly = mainAssembly,
                    ExtraAssemblies = new Assembly[0],
                    Middlewares = new INoaMiddleware[] {new NoaNetworkLoggingMiddleware(), new NoaFragmentationMiddleware(), new NoaRsaMiddleware()}
                });
        }

        /// <summary>
        /// Only use if you need a setup that definitely differs from the default setup!
        /// </summary>
        /// <param name="config">Configuration Class Instance</param>
        internal static void CustomSetup(MultiNoaConfig config)
        {
            Setup(config);
        }


        private static void Setup(MultiNoaConfig config)
        {
            if (_setupDone)
            {
                MultiNoaLoggingManager.Logger.Warning($"Tried to set up multiNoa a second time.\n{Environment.StackTrace}");
                return;
            }
            _setupDone = true;
            
            MultiNoaLoggingManager.Logger.Information(Constants.Ascii);
            MultiNoaLoggingManager.Logger.Information($"Using multiNoa {VersionCode}");

            RegisterMiddlewares(config.Middlewares);


            if (config.DataBufferSize < 4096)
            {
                MultiNoaLoggingManager.Logger.Warning("Using a buffer size of under 4096! This is not supported. Use something bigger for a more reliable experience");
            }
            
            

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
            var checkingMiddlewares = new List<INoaMiddleware>();
            var encryptingMiddlewares = new List<INoaMiddleware>();
            var fragmentingMiddlewares = new List<INoaMiddleware>();
            var correctingMiddlewares = new List<INoaMiddleware>();
            var nonModifyingMiddlewares = new List<INoaMiddleware>();

            foreach (var middleware in middlewares)
            {
                switch (middleware.GetTarget())
                {
                    case MiddlewareTarget.Checking:
                        checkingMiddlewares.Add(middleware);
                        break;
                    case MiddlewareTarget.Encrypting:
                        encryptingMiddlewares.Add(middleware);
                        break;
                    case MiddlewareTarget.Fragmenting:
                        fragmentingMiddlewares.Add(middleware);
                        break;
                    case MiddlewareTarget.Correcting:
                        correctingMiddlewares.Add(middleware);
                        break;
                    case MiddlewareTarget.NonModifying:
                        nonModifyingMiddlewares.Add(middleware);
                        break;
                    
                    case MiddlewareTarget.Dummy:
                    default:
                        break;
                }
                
                middleware.Setup();
            }

            CheckingMiddlewares = checkingMiddlewares.ToArray();
            EncryptingMiddlewares = encryptingMiddlewares.ToArray();
            FragmentingMiddlewares = fragmentingMiddlewares.ToArray();
            CorrectingMiddlewares = correctingMiddlewares.ToArray();
            NonModifyingMiddlewares = nonModifyingMiddlewares.ToArray();


            if (FragmentingMiddlewares.Length == 0)
            {
                MultiNoaLoggingManager.Logger.Warning("You don't seem to have defined any fragmenting middleware. This may cause unexpected behaviour. Please use NoaFragmentationMiddleware or any original implementation to be save!");
            }

            if (EncryptingMiddlewares.Length == 0)
            {
                MultiNoaLoggingManager.Logger.Warning("You are sending all data unencrypted. This is not a big problem in most cases, but make sure to never ever send ANY confidential data in this configuration");
            }
        }
    }

    public class MultiNoaSetupCollection
    {
        internal MultiNoaSetupCollection(){}

        /// <summary>
        /// Used for a fully custom setup.
        /// </summary>
        /// <param name="config">Config to follow</param>
        public void CustomSetup(MultiNoaConfig config)
        {
            MultiNoaSetup.CustomSetup(config);
        }
    }

    public static class DefaultSetupExtension
    {
        /// <summary>
        /// Sets up MultiNoa in a way fitting for most use cases, given that all packet types + handlers are defined within mainAssembly.
        /// </summary>
        /// <param name="mainAssembly"></param>
        public static void DefaultSetup(this MultiNoaSetupCollection collection, Assembly mainAssembly)
        {
            MultiNoaSetup.DefaultSetup(mainAssembly);
        }
    }
    

    public struct MultiNoaConfig
    {
        public INoaMiddleware[] Middlewares;
        public Assembly[] ExtraAssemblies;
        public Assembly MainAssembly;
        public int DataBufferSize;

        public MultiNoaConfig(Assembly mainAssembly, Assembly[] extraAssemblies, INoaMiddleware[] middlewares, int dataBufferSize)
        {
            MainAssembly = mainAssembly;
            ExtraAssemblies = extraAssemblies;
            Middlewares = middlewares;
            DataBufferSize = dataBufferSize;
        }
        
        
    }



    internal static class Constants
    {
        public const string Ascii = @"

#*                                      
 ####                                   
   #####                                
     ######                             
       #######                 ,,       
        #########             ,,,       
       ***##########        ***,,       
       ///**###########   ******* *##.  
       //////*/##########********#####  
       /////////*#######,  *****######( 
        //////////  ###       ######### 
          ////////////***   ########### 
            /////////////**#############
              ////////////   ###########
               /////////        ########
                 //////            #####
                  ///                 ##
";
    }
    

}