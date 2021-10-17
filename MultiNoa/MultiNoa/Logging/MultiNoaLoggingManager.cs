using System;

namespace MultiNoa.Logging
{
     public static class MultiNoaLoggingManager
    {
        /// <summary>
        /// Used to log messages inside MultiNOA. Set this to your own implementation if you want to use a custom logging system (which would be recommended)
        /// The default logger is very (and I mean that) basic.
        /// </summary>
        public static ILogger Logger = new DefaultLogger();
        
        
        public static void TestMsg()
        {
            Logger.Debug("Hi");
            Logger.Verbose("Hi");
            Logger.Information("Hi");
            Logger.Warning("Hi");
            Logger.Error("Hi");
            Logger.Fatal("Hi");
        }
        
        private class DefaultLogger: ILogger
        {
            public void Debug(string logMessage)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("[D] " + logMessage);
                Console.ResetColor();
            }

            public void Verbose(string logMessage)
            {
                Console.ForegroundColor = ConsoleColor.Gray;
                Console.WriteLine("[V] " + logMessage);
                Console.ResetColor();
            }

            public void Information(string logMessage)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("[I] " + logMessage);
                Console.ResetColor();
            }

            public void Warning(string logMessage)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("[W] " + logMessage);
                Console.ResetColor();
            }

            public void Error(string logMessage)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.WriteLine("[E] " + logMessage);
                Console.ResetColor();
            }

            public void Fatal(string logMessage)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("[F] " + logMessage);
                Console.ResetColor();
            }
        }
    }
     
     public interface ILogger
     {
         void Debug(string logMessage);
         void Verbose(string logMessage);
         void Information(string logMessage);
         void Warning(string logMessage);
         void Error(string logMessage);
         void Fatal(string logMessage);
     }
}