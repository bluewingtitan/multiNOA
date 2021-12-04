using System;

namespace MultiNoa.Logging
{
    
     public static class MultiNoaLoggingManager
    {
        /// <summary>
        /// Used to log messages inside MultiNOA. Set this to your own implementation to use with your own logging system.
        /// The default logger is very (and I mean that) basic.
        /// </summary>
        public static ILogger Logger = new NoaDefaultLogger();
        
        /// <summary>
        /// Used to log a simple text in every style.
        /// </summary>
        public static void TestMsg(string msg = "Test")
        {
            Logger.Debug(msg);
            Logger.Verbose(msg);
            Logger.Information(msg);
            Logger.Warning(msg);
            Logger.Error(msg);
            Logger.Fatal(msg);
        }


        /// <summary>
        /// A logger that simply cancels all output.
        /// </summary>
        public class NoaNoLogger : ILogger
        {
            public void Debug(string logMessage)
            {
                return;
            }

            public void Verbose(string logMessage)
            {
                return;
            }

            public void Information(string logMessage)
            {
                return;
            }

            public void Warning(string logMessage)
            {
                return;
            }

            public void Error(string logMessage)
            {
                return;
            }

            public void Fatal(string logMessage)
            {
                return;
            }
        }
        
        public class NoaDefaultLogger: ILogger
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