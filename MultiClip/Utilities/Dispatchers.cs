using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace MultiClip.Utilities
{
    public static class Dispatchers
    {
        public static Action InvokeRepeating(this Dispatcher dispatcher, Action action, int delayMs, int repeatMs)
        {
            bool isCanceled = false;
            dispatcher.BeginInvoke(new Action(async () =>
            {
                await Task.Delay(delayMs);
                while (!isCanceled)
                {
                    await dispatcher.InvokeAsync(action);
                    await Task.Delay(repeatMs);
                }
            }));

            return () => isCanceled = true;
        }

        /// <summary>
        /// Retries the action on the dispatcher asynchronously according
        /// to the maximum number of retries, set apart by the specified delay.
        /// If the operation fails, throws the last exception thrown by the action.
        /// </summary>
        public static Task RetryOnErrorAsync(this Dispatcher dispatcher, Action action, int retryCount, int delayMs)
        {
            Contract.Requires(dispatcher != null);
            Contract.Requires(action != null);

            return dispatcher.RetryOnErrorAsync<object>(() =>
            {
                action();
                return null;
            }, retryCount, delayMs);
        }

        /// <summary>
        /// Retries the action on the dispatcher asynchronously according
        /// to the maximum number of retries, set apart by the specified delay.
        /// If the operation fails, throws the last exception thrown by the action.
        /// </summary>
        public static Task<T> RetryOnErrorAsync<T>(this Dispatcher dispatcher, Func<T> action, int retryCount, int delayMs)
        {
            Contract.Requires(dispatcher != null);
            Contract.Requires(action != null);

            DateTime startTime = DateTime.Now;
            TaskCompletionSource<T> taskCompletionSource = new TaskCompletionSource<T>();
            Task<T> task = taskCompletionSource.Task;

            int i = retryCount;
            
            Action retryAction = null;
            retryAction = () =>
            {
                T result = default;
                try
                {
                    result = action();
                }
                catch (Exception e)
                {
                    if (i-- > 0)
                    {
                        dispatcher.BeginInvoke(retryAction);
                        return;
                    }
                    else
                    {
                        taskCompletionSource.SetException(e);
                        return;
                    }
                }

                taskCompletionSource.SetResult(result);
            };

            retryAction();
            return task;
        }

    }
}
