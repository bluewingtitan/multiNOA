namespace MultiNoa.Profiling.Timing
{
    public interface ITimer
    {
        public delegate ITimer TimerCreationDelegate(string name);
        
        /// <summary>
        /// Stops timer
        /// </summary>
        /// <returns>Milliseconds passed since start</returns>
        double Stop();
    }
}