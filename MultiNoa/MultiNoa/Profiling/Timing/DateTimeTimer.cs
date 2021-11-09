using System;
using MultiNoa.Logging;

namespace MultiNoa.Profiling.Timing
{
    public class DateTimeTimer : ITimer
    {
        private readonly string _name;
        private readonly bool _log;
        private readonly DateTime _dateTime;

        public DateTimeTimer(string name, bool log = true)
        {
            _name = name;
            _log = log;
            _dateTime = DateTime.Now;
        }

        public double Stop()
        {
            var r = (DateTime.Now - _dateTime).TotalMilliseconds;
            if(_log)
                MultiNoaLoggingManager.Logger.Verbose($"TIMER:[{_name}] {r}ms");
            return r;
        }
    }
}