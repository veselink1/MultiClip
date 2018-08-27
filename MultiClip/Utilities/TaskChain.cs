using System;
using System.Threading;
using System.Threading.Tasks;

namespace MultiClip.Utilities
{
    /// <summary>
    /// Represents a chain of Tasks, which can be added to.
    /// </summary>
    public class TaskChain
    {
        private Task _currentTask;

        public bool IsCompleted => _currentTask.IsCompleted;

        public TaskChain()
        {
            _currentTask = Task.FromResult<object>(null);
        }

        public Task AddTask(Action action)
        {
            SynchronizationContext context = SynchronizationContext.Current;
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            _currentTask = _currentTask.ContinueWith(t =>
            {
                context.Send(delegate
                {
                    try
                    {
                        action();
                        tcs.SetResult(null);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                }, null);
            });
            return tcs.Task;
        }

        public Task AddTask(Func<Task> action)
        {
            SynchronizationContext context = SynchronizationContext.Current;
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            _currentTask = _currentTask.ContinueWith(t =>
            {
                context.Send(async delegate
                {
                    try
                    {
                        await action();
                        tcs.SetResult(null);
                    }
                    catch (Exception e)
                    {
                        tcs.SetException(e);
                    }
                }, null);
            });
            return tcs.Task;
        }
    }
}
