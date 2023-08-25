using System;
using System.Threading;
using System.Threading.Tasks;

namespace GIT.Utilities
{
    public static class TaskRunWithTimedCallback
    {
        /// <summary>
        /// Runs the given task and, after each interval specified in tickIntervals, runs the tickDelegate action in a separate thread until the original task is finished.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="tickIntervals"></param>
        /// <param name="tickDelegate"></param>
        /// <returns></returns>
        public static Task RunWithTimedCallback(this Task task, TimeSpan tickIntervals, Action tickDelegate)
        {
            return RunWithTimedCallbackInternal(task, tickIntervals, WrapAsTask(tickDelegate));
        }
        /// <summary>
        /// Runs the given task and, after each interval specified in tickIntervals, runs the tickDelegate action in a separate thread until the original task is finished.
        /// </summary>
        /// <param name="task"></param>
        /// <param name="tickIntervals"></param>
        /// <param name="tickDelegate"></param>
        /// <returns></returns>
        public static Task RunWithTimedCallback(this Task task, TimeSpan tickIntervals, Func<Task> tickDelegate)
        {
            return RunWithTimedCallbackInternal(task, tickIntervals, tickDelegate);
        }
        /// <summary>
        /// Runs the given task and, after each interval specified in tickIntervals, runs the tickDelegate action in a separate thread until the original task is finished.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="tickIntervals"></param>
        /// <param name="tickDelegate"></param>
        /// <returns></returns>
        public static Task<T> RunWithTimedCallback<T>(this Task<T> task, TimeSpan tickIntervals, Action tickDelegate)
        {
            return RunWithTimedCallbackInternal(task, tickIntervals, WrapAsTask(tickDelegate));
        }
        /// <summary>
        /// Runs the given task and, after each interval specified in tickIntervals, runs the tickDelegate action in a separate thread until the original task is finished.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="task"></param>
        /// <param name="tickIntervals"></param>
        /// <param name="tickDelegate"></param>
        /// <returns></returns>

        public static Task<T> RunWithTimedCallback<T>(this Task<T> task, TimeSpan tickIntervals, Func<Task> tickDelegate)
        {
            return RunWithTimedCallbackInternal(task, tickIntervals, tickDelegate);
        }

        #region implementation
        // Simple wrapper to allow the use of a synchroneous action as an asynchroneous one.
        private static Func<Task> WrapAsTask(Action action)
        {
            return () =>
            {
                action();
                return Task.CompletedTask;
            };
        }

        private static async Task<TResult> RunWithTimedCallbackInternal<T, TResult>(
            this Task<T> task,
            TimeSpan tickIntervals,
            Func<Task> tickDelegate,
            Func<Task<T>, TResult> resultSelector)
        {
            if (task.IsCompleted) // If the source task has already completed
                return resultSelector(task);
            var cancellationSource = new CancellationTokenSource();
            var tickTask = Task.Run(async () =>
            {
                while (!cancellationSource.Token.IsCancellationRequested)
                {
                    await Task.Delay(tickIntervals, cancellationSource.Token);
                    await tickDelegate();
                }
            }, cancellationSource.Token);

            // Wait for either the main task or the tick task to complete
            await Task.WhenAny(task, tickTask);

            // If the main task has completed, cancel the tick task
            cancellationSource.Cancel();
            try
            {
                await tickTask;
            }
            catch (OperationCanceledException) { /* Ignored */ }

            // Wait for the main task in case it hasn't completed yet
            await task;
            return resultSelector(task);
        }

        private static async Task<T> RunWithTimedCallbackInternal<T>(this Task<T> task, TimeSpan tickIntervals, Func<Task> tickDelegate)
        {
            return await RunWithTimedCallbackInternal(task, tickIntervals, tickDelegate, t => t.Result);
        }

        private static async Task RunWithTimedCallbackInternal(this Task task, TimeSpan tickIntervals, Func<Task> tickDelegate)
        {
            await RunWithTimedCallbackInternal<object, object>(task.ContinueWith(t => (object)null), tickIntervals, tickDelegate, _ => default);
        } 
        #endregion
    }
}
