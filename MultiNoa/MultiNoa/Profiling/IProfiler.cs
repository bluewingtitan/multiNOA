namespace MultiNoa.Profiling
{
    public interface IProfiler
    {
        
        /// <summary>
        /// Starts a timer under a set name
        /// </summary>
        /// <param name="context">Context Name</param>
        void Start(string context);
        
        /// <summary>
        /// Stops the lastly started timer
        /// </summary>
        void Stop();

        /// <summary>
        /// Stops all timers and prints out the full measurements into console
        /// </summary>
        void StopAllAndPrint();

        void Reset();

    }
}