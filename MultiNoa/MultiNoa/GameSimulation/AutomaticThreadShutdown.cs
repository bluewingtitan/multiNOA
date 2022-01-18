namespace MultiNoa.GameSimulation
{
    /// <summary>
    /// Utility to easily shut down a dynamic thread after x full seconds.
    /// </summary>
    public class AutomaticThreadShutdown: IUpdatable
    {
        private ulong _secondsUntilShutdown;
        private readonly IDynamicThread _thread;

        public AutomaticThreadShutdown(IDynamicThread thread, ulong secondsUntilShutdown)
        {
            _thread = thread;
            _secondsUntilShutdown = secondsUntilShutdown;
        }

        public void PerSecondUpdate()
        {
            _secondsUntilShutdown--;
            if(_secondsUntilShutdown > 0) return;
            
            _thread.Stop();
            _thread.RemoveUpdatable(this);
        }
    }
}