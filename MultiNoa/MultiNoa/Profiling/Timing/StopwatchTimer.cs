using System.Diagnostics;
using MultiNoa.Logging;

namespace MultiNoa.Profiling.Timing
{
    public class StopwatchTimer: ITimer
    {
        private readonly string _name;
        private readonly bool _log;
        private readonly Stopwatch _stopwatch;

        public StopwatchTimer(string name, bool log = true)
        {
            _name = name;
            _log = log;
            _stopwatch = new Stopwatch();
            _stopwatch.Start();
        }

        public double Stop()
        {
            _stopwatch.Stop();;
            var r = _stopwatch.Elapsed.TotalMilliseconds;
            if(_log)
                MultiNoaLoggingManager.Logger.Verbose($"TIMER:[{_name}] {r}ms");
            return r;
        }
    }
}