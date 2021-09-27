using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using MultiNOA.Middleware;

namespace MultiNOA.GameSimulation
{
    public class DynamicThread
    {
        public static bool AreRunning { get; private set; }

        /// <summary>
        /// Stops all running dynamic threads => stops packet handling and handling of new connections.
        /// </summary>
        public static void StopAll() => AreRunning = false;
        
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



        private readonly List<Action> executeOnThread = new List<Action>();
        private readonly List<Action> ExecuteCopiedOnThread = new List<Action>();
        private bool _actionToExecuteOnMainThread = false;
        
        /**
         * Schedule an execution of an action.
         * This action will be executed on the next tick BEFORE doing the normal Updates.
         */
        public void ScheduleExecution(Action action)
        {
            if (action == null)
            {
                MultiNoaLoggingManager.Logger.Verbose("Tried to execute action on thread but there was none.");
                return;
            }
    
            lock (executeOnThread)
            {
                executeOnThread.Add(action);
                _actionToExecuteOnMainThread = true;
            }
        }
        
        private void ExecuteAll()
        {
            if (!_actionToExecuteOnMainThread) return;
            ExecuteCopiedOnThread.Clear();
            
            lock (executeOnThread)
            {
                ExecuteCopiedOnThread.AddRange(executeOnThread);
                executeOnThread.Clear();
                _actionToExecuteOnMainThread = false;
            }
    
            foreach (var action in ExecuteCopiedOnThread)
            {
                action();
            }
        }


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

            var tickStart = DateTime.Now;
            
            DateTime nextLoop = DateTime.Now;
            int ticksBehind = 0;
            
            while (AreRunning && IsRunning)
            {
                while (nextLoop < DateTime.Now)
                {
                    tickStart = DateTime.Now;
                    ExecuteAll();
                    Update();
                    
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
            ScheduleExecution((() =>
            {
                if(Updatables.Contains(updatable)) return;
                Updatables.Add(updatable);
            }));
        }

        public void RemoveUpdatable(IUpdatable updatable)
        {
            ScheduleExecution((() => Updatables.Remove(updatable)));
        }

        public void ClearUpdatables()
        {
            ScheduleExecution((() => Updatables.Clear()));
        }

        public bool ContainsUpdatable(IUpdatable updatable)
        {
            return Updatables.Contains(updatable);
        }
        
        
        private List<IOffsetTask> OffsetTasks { get; } = new List<IOffsetTask>();

        public void AddOffsetTask(IOffsetTask task)
        {
            ScheduleExecution(() => OffsetTasks.Add(task));
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
                    ScheduleExecution(() => OffsetTasks.Remove(task));
                }
            }
        }
    }
    
    public interface IUpdatable
    {
        public void Update();
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