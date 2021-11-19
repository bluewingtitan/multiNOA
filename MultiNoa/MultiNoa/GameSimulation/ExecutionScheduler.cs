using System;
using System.Collections.Generic;
using MultiNoa.Logging;

namespace MultiNoa.GameSimulation
{
    /// <summary>
    /// Utility to save actions in a thread save way for execution at another time
    /// </summary>
    public class ExecutionScheduler
    {
        private readonly List<Action> executeOnThread = new List<Action>();
        private readonly List<Action> ExecuteCopiedOnThread = new List<Action>();
        private bool _actionToExecuteOnMainThread = false;
        
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
        
        public void ExecuteAll()
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
    }
}