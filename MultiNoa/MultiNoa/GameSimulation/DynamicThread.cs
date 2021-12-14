using System;
using System.Collections.Generic;
using System.Threading;
using MultiNoa.Logging;

namespace MultiNoa.GameSimulation
{
    public class DynamicThread:IDynamicThread
    {
        /**
         * Use DynamicThread.Stop() to stop this thread.
         */
        public bool IsRunning { get; private set; } = true;

        private readonly Thread _runningThread;

        public readonly double TimePerTick;

        public string ThreadName { get;}
        
        public int GoalTPS;

        /// <summary>
        /// Percentile Usage of Time (How close to the limit is this thread? Or maybe even over?)
        /// 0 => uses 0% of TimePerTick => Sleeping TimePerTick seconds per tick
        /// 1 => uses 100% of TimePerTick => Does not sleep
        /// 2 => uses 200% => Missing every second tick!
        /// 2.2 => uses 220%...
        /// </summary>
        public double TimeUsage { get; private set; } = 0;



        private readonly ExecutionScheduler _scheduler = new ExecutionScheduler();

        /**
         * Creates and Starts a Dynamic Thread, used to execute tasks in a controlled manner.
         */
        public DynamicThread(int tps, string threadName)
        {
            ThreadName = threadName;
            
            TimePerTick = 1000d / tps;
            GoalTPS = (int) Math.Round(1000 / TimePerTick);

            _runningThread = new Thread(ThreadFunction) {Name = ThreadName};
            _runningThread.Start();
        }

        public void Stop()
        {
            IsRunning = false;
        }
        
        private void ThreadFunction()
        {
            MultiNoaLoggingManager.Logger.Information($"Thread {ThreadName} was started successfully.");

            var nextLoop = DateTime.Now;
            var ticksBehind = 0;
            var ticksUntilSecondTick = GoalTPS;
            
            while (IDynamicThread.AreRunning && IsRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    var tickStart = DateTime.Now;
                    _scheduler.ExecuteAll();
                    Update();
                    
                    ticksUntilSecondTick--;

                    if (ticksUntilSecondTick <= 0)
                    {
                        ticksUntilSecondTick = GoalTPS;
                        PerSecondUpdates();
                    }
                    
                    
                    #region Tick management

                    if (ticksBehind > GoalTPS) // If at least one second behind, skip all ticks remaining
                    {
                        MultiNoaLoggingManager.Logger.Information($"{ThreadName} was running {ticksBehind} behind and skipepd {ticksBehind * TimePerTick}ms.");
                        ticksBehind = 0;
                        nextLoop = DateTime.Now;
                    }
                    
                    
                    nextLoop = nextLoop.AddMilliseconds(TimePerTick);
                    

                    var tickTimeUsage = (TimeUsage + (DateTime.Now - tickStart).TotalSeconds) / TimePerTick;

                    TimeUsage = (tickTimeUsage + TimeUsage) / 2d;

                    if (nextLoop > DateTime.Now)
                    {
                        if (ticksBehind > 0) ticksBehind = 0;
                        var timeout = nextLoop - DateTime.Now;
                        if(!(timeout.Milliseconds <= 0))
                            Thread.Sleep(timeout);
                    }
                    else
                    {
                        ticksBehind++;
                    }

                    #endregion
                }
            }
            
            MultiNoaLoggingManager.Logger.Information($"Thread {ThreadName} was closed successfully.");
        }

        /**
         * All Logic that has to be executed with each Tick can be written inside Updatables
         * by using IUpdatable. This enables easy, dynamic, per-tick functionality.
         *
         * This Function executes Update() for each IUpdatable in the Updatables list and runs each tick.
         */
        private List<IUpdatable> Updatables { get; } = new List<IUpdatable>();

        /// <summary>
        /// Adds an updatable to the threads updatables if not there allready.
        /// Does nothing if allready registered => No double registration.
        /// </summary>
        /// <param name="updatable">Updatable to add</param>
        public void AddUpdatable(IUpdatable updatable)
        {
            _scheduler.ScheduleExecution((() =>
            {
                if(Updatables.Contains(updatable)) return;
                Updatables.Add(updatable);
            }));
        }

        public void RemoveUpdatable(IUpdatable updatable)
        {
            _scheduler.ScheduleExecution((() => Updatables.Remove(updatable)));
        }

        public void ClearUpdatables()
        {
            _scheduler.ScheduleExecution((() => Updatables.Clear()));
        }

        public bool ContainsUpdatable(IUpdatable updatable)
        {
            return Updatables.Contains(updatable);
        }
        
        
        private List<IOffsetTask> OffsetTasks { get; } = new List<IOffsetTask>();

        public void AddOffsetTask(IOffsetTask task)
        {
            _scheduler.ScheduleExecution(() => OffsetTasks.Add(task));
        }
        
        private void PerSecondUpdates()
        {
            foreach (var updatable in Updatables)
            {
                updatable.PerSecondUpdate();
            }
        }
        
        private void Update()
        {
            foreach (var updatable in Updatables)
            {
                updatable.Update();
            }

            foreach (var task in OffsetTasks)
            {
                if (task.Tick() <= 0)
                {
                    task.Execute();
                    _scheduler.ScheduleExecution(() => OffsetTasks.Remove(task));
                }
            }
        }
    }
    
    public interface IUpdatable
    {
        /// <summary>
        /// Called with each tick
        /// </summary>
        public void Update();
        
        /// <summary>
        /// Called once a second
        /// </summary>
        public void PerSecondUpdate();
    }
    
    /// <summary>
    /// Kind of like IUpdatable, but used for a single-run function that is to be offset without the need of asynchronous code (=> Thread-safe).
    /// </summary>
    public interface IOffsetTask
    {
        /// <summary>
        /// Tells the underlying object, that a tick happened.
        /// </summary>
        /// <returns>Amount of ticks remaining until Execute() should be executed</returns>
        public int Tick();

        /// <summary>
        /// Will be called once Tick() returns 0.
        /// </summary>
        public void Execute();
    }
    
    
    /// <summary>
    /// Basic implementation of IOffsetTask.
    /// </summary>
    public class OffsetAction : IOffsetTask
    {
        private readonly Action _action;
        private int _ticksToWait;

        public OffsetAction(Action action, int ticksToWait)
        {
            _action = action;
            _ticksToWait = ticksToWait;
        }


        /// <summary>
        /// Tells the underlying object, that a tick happened.
        /// </summary>
        /// <returns>Amount of ticks remaining until Execute() should be executed</returns>
        public int Tick()
        {
            _ticksToWait--;
            return (_ticksToWait<0)? 0 : _ticksToWait; // Never fall under 0.
        }

        /// <summary>
        /// Will be called once Tick() returns 0.
        /// </summary>
        public void Execute()
        {
            _action();
        }
    }
}