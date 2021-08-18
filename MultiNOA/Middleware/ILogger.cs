namespace MultiNOA.Middleware
{
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