using System;
using System.Collections.Generic;
using System.Threading;
using MultiNoa.Logging;
using MultiNoa.Profiling.Timing;

namespace MultiNoa.Profiling
{
    /// <summary>
    /// Can be used to measure the execution time of a row of functions.
    /// </summary>
    public class CallstackProfiler: IProfiler
    {
        private const string IndentationString = "\t";
        private List<string> _indentations = new List<string>();
        
        public static ITimer.TimerCreationDelegate TimerCreator = ctxName => new DateTimeTimer(ctxName, false);

        private bool _stopped = false;
        private Stack<TimerData> _startedTimers = new Stack<TimerData>();
        private Stack<TimerData> _finishedTimers = new Stack<TimerData>();

        private int _indentation = 0;
        
        // -[Demo.One]: 45ms
        //    L[Demo.Two]: 10ms
        //    L[Demo.Three]: 15ms
        //        L[Demo.Four]: 5ms
        //        L[Demo.Four]: 6ms
        //    L[Demo.Six]: 15ms

        public void Start(string context)
        {
            _startedTimers.Push(new TimerData(context, _indentation));
            _indentation++;
        }

        public void Stop()
        {
            if(_startedTimers.Count <= 0)
                throw new NullReferenceException("No started timer to stop");
            
            if(_stopped)
                throw new Exception("The profiler was stopped. Reset it to use it again.");
            
            var popped = _startedTimers.Pop();
            
            popped.Stop();
            
            _finishedTimers.Push(popped);
            _indentation--;
        }

        
        public void StopAllAndPrint()
        {
            _stopped = true;
            while (_startedTimers.Count > 0)
            {
                var popped = _startedTimers.Pop();
                _finishedTimers.Push(popped);
            }

            // Put printing into separate thread.
            var t = new Thread(() =>
            {
                string s = "";
                foreach (var timer in _finishedTimers)
                {
                    if (timer.Indentation == 0)
                    {
                        s += "-"; // First Layer Function Call.
                    }
                    else
                    {
                        var indentation = _indentations[timer.Indentation];
                        if (indentation != null)
                        {
                            s += indentation;
                        }
                        else
                        {
                            var newIndentation = "";
                            for (int i = 0; i < timer.Indentation; i++)
                                newIndentation += IndentationString;

                            _indentations[timer.Indentation] = newIndentation;
                            s += newIndentation;
                        }
                        

                        s += "L";
                    }

                    s += "[" + timer.ContextName + "]: " + Math.Round(timer.FinalTime,2) + "ms\n";
                }
            
                MultiNoaLoggingManager.Logger.Information(s);
            });

            t.Start();
        }

        public void Reset()
        {
            _stopped = false;
            _startedTimers = new Stack<TimerData>();
            _finishedTimers = new Stack<TimerData>();
            _indentation = 0;
        }
    }



    internal class TimerData
    {
        /// <summary>
        /// Constructs the timer data around the timer.
        /// </summary>
        /// <param name="contextName">Context Name to use</param>
        /// <param name="indentation">Amount of indentation needed later</param>
        public TimerData(string contextName, int indentation)
        {
            Timer = CallstackProfiler.TimerCreator(contextName);
            ContextName = contextName;
            Indentation = indentation;
        }
        
        public int Indentation { get; }
        public string ContextName { get; }
        private ITimer Timer { get;}
        public double FinalTime { get; private set; } = -1;
        public bool Finished { get; private set; } = false;

        public void Stop()
        {
            FinalTime = Timer.Stop();
            Finished = true;
        }
        
    }


}