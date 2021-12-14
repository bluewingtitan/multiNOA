namespace MultiNoa.GameSimulation
{
    public interface IDynamicThread
    {
        public static bool AreRunning { get; private set; } = true;

        /// <summary>
        /// Stops all running instances of IDynamicThread => not reversable, only use as part of a full stop-routine!
        /// </summary>
        public static void StopAll() => AreRunning = false;
        
        
        void Stop();
        void AddUpdatable(IUpdatable updatable);
        void RemoveUpdatable(IUpdatable updatable);
        void ClearUpdatables();
        bool ContainsUpdatable(IUpdatable updatable);
        void AddOffsetTask(IOffsetTask task);
        
    }
}