using System;
using System.Threading;
using System.Threading.Tasks;

namespace RetryHelper
{
    /// <summary>
    /// Helper class to retry work until the retrying is cancelled.
    /// </summary>
    public class Retry
    {
        #region Methods

        /// <summary>
        /// Retries an <see cref="Action"/> synchronously.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to retry.</param>
        /// <param name="delay">The delay retries are done.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used to control the cancellation
        /// of the retry loop.
        /// </param>
        /// <param name="progress">
        /// The <see cref="IProgress{T}"/> to monitor the state of the retry loop.
        /// </param>
        public static void RetryAction(Action action
            , TimeSpan delay
            , CancellationToken cancellationToken
            , IProgress<bool> progress = null)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                Thread.Sleep(delay);
                bool success = false;

                try
                {
                    action();
                    success = true;
                }
                catch { }

                if (!(progress is null))
                    progress.Report(success);
            }
        }

        /// <summary>
        /// Retries an <see cref="Action"/> asynchronously.
        /// </summary>
        /// <param name="action">The <see cref="Action"/> to retry.</param>
        /// <param name="delay">The delay retries are done.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used to control the cancellation
        /// of the retry loop.
        /// </param>
        /// <param name="progress">
        /// The <see cref="IProgress{T}"/> to monitor the state of the retry loop.
        /// </param>
        public static async Task RetryActionAsync(Action action
            , TimeSpan delay
            , CancellationToken cancellationToken
            , IProgress<bool> progress = null)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(delay, cancellationToken);
                bool success = false;

                try
                {
                    action();
                    success = true;
                }
                catch { }

                if (!(progress is null))
                    progress.Report(success);
            }
        }

        /// <summary>
        /// Retries an <see cref="Action{T}"/> synchronously with a parameter.
        /// </summary>
        /// <param name="action">The <see cref="Action{T}"/> to retry.</param>
        /// <param name="param">Parameter of the <see cref="Action{T}"/></param>
        /// <param name="delay">The delay retries are done.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used to control the cancellation
        /// of the retry loop.
        /// </param>
        /// <param name="progress">
        /// The <see cref="IProgress{T}"/> to monitor the state of the retry loop.
        /// </param>
        public static void RetryAction<T>(Action<T> action
            , T param
            , TimeSpan delay
            , CancellationToken cancellationToken
            , IProgress<bool> progress = null)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                Thread.Sleep(delay);
                bool success = false;

                try
                {
                    action(param);
                    success = true;
                }
                catch { }

                if (!(progress is null))
                    progress.Report(success);
            }
        }

        /// <summary>
        /// Retries an <see cref="Action{T}"/> asynchronously with a parameter.
        /// </summary>
        /// <param name="action">The <see cref="Action{T}"/> to retry.</param>
        /// <param name="param">Parameter of the <see cref="Action{T}"/></param>
        /// <param name="delay">The delay retries are done.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used to control the cancellation
        /// of the retry loop.
        /// </param>
        /// <param name="progress">
        /// The <see cref="IProgress{T}"/> to monitor the state of the retry loop.
        /// </param>
        public static async Task RetryActionAsync<T>(Action<T> action
            , T param
            , TimeSpan delay
            , CancellationToken cancellationToken
            , IProgress<bool> progress = null)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(delay, cancellationToken);
                bool success = false;

                try
                {
                    action(param);
                    success = true;
                }
                catch { }

                if (!(progress is null))
                    progress.Report(success);
            }
        }

        /// <summary>
        /// Retries an <see cref="Func{Task}"/> asynchronously with a parameter.
        /// </summary>
        /// <param name="func">The <see cref="Func{Task}"/> to retry.</param>
        /// <param name="delay">The delay retries are done.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used to control the cancellation
        /// of the retry loop.
        /// </param>
        /// <param name="progress">
        /// The <see cref="IProgress{T}"/> to monitor the state of the retry loop.
        /// </param>
        public static async Task RetryFuncAsync(Func<Task> func
            , TimeSpan delay
            , CancellationToken cancellationToken
            , IProgress<bool> progress = null)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(delay, cancellationToken);
                bool success = false;

                try
                {
                    var task = func();

                    if (task.Status == TaskStatus.Created)
                        task.Start();

                    await task;
                    success = true;
                }
                catch { }

                if (!(progress is null))
                    progress.Report(success);
            }
        }

        /// <summary>
        /// Retries an <see cref="Func{T, Task}"/> asynchronously with a parameter.
        /// </summary>
        /// <param name="func">The <see cref="Func{T, Task}"/> to retry.</param>
        /// <param name="param">Parameter of the <see cref="Func{T, Task}"/></param>
        /// <param name="delay">The delay retries are done.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used to control the cancellation
        /// of the retry loop.
        /// </param>
        /// <param name="progress">
        /// The <see cref="IProgress{T}"/> to monitor the state of the retry loop.
        /// </param>
        public static async Task RetryFuncAsync<T>(Func<T, Task> func
            , T param
            , TimeSpan delay
            , CancellationToken cancellationToken
            , IProgress<bool> progress = null)
        {
            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(delay, cancellationToken);
                bool success = false;

                try
                {
                    var task = func(param);

                    if (task.Status == TaskStatus.Created)
                        task.Start();

                    await task;
                    success = true;
                }
                catch { }

                if (!(progress is null))
                    progress.Report(success);
            }
        }

        /// <summary>
        /// Retries an <see cref="Func{Task{T}}"/> synchronously and return the
        /// result of the task.
        /// </summary>
        /// <param name="func">The <see cref="Func{Task{T}}"/> to retry.</param>
        /// <param name="delay">The delay retries are done.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used to control the cancellation
        /// of the retry loop.
        /// </param>
        /// <param name="progress">
        /// The <see cref="IProgress{T}"/> to monitor the state of the retry loop.
        /// </param>
        public static T RetryFunc<T>(Func<Task<T>> func
            , TimeSpan delay
            , CancellationToken cancellationToken
            , IProgress<bool> progress = null)
        {
            T result = default;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                Task.Delay(delay, cancellationToken)
                    .GetAwaiter()
                    .GetResult();

                bool success = false;

                try
                {
                    var task = func();

                    if (task.Status == TaskStatus.Created)
                        task.Start();

                    result = task
                       .GetAwaiter()
                       .GetResult();

                    success = true;
                }
                catch { }

                if (!(progress is null))
                    progress.Report(success);

                if (success)
                    break;
            }

            return result;
        }

        /// <summary>
        /// Retries an <see cref="Func{T, Task{TResult}}"/> synchronously and
        /// return the result of the task.
        /// </summary>
        /// <param name="func">
        /// The <see cref="Func{T, Task{TResult}}"/> to retry.
        /// </param>
        /// <param name="param">Parameter of the <see cref="Func{T, Task{TResult}}"/></param>
        /// <param name="delay">The delay retries are done.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used to control the cancellation
        /// of the retry loop.
        /// </param>
        /// <param name="progress">
        /// The <see cref="IProgress{T}"/> to monitor the state of the retry loop.
        /// </param>
        public static TResult RetryFunc<T, TResult>(Func<T, Task<TResult>> func
            , T param
            , TimeSpan delay
            , CancellationToken cancellationToken
            , IProgress<bool> progress = null)
        {
            TResult result = default;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                Task.Delay(delay, cancellationToken)
                    .GetAwaiter()
                    .GetResult();

                bool success = false;

                try
                {
                    var task = func(param);

                    if (task.Status == TaskStatus.Created)
                        task.Start();

                    result = task
                       .GetAwaiter()
                       .GetResult();

                    success = true;
                }
                catch { }

                if (!(progress is null))
                    progress.Report(success);

                if (success)
                    break;
            }

            return result;
        }

        /// <summary>
        /// Retries an <see cref="Func{Task{T}}"/> asynchronously.
        /// </summary>
        /// <param name="func">The <see cref="Func{Task{T}}"/> to retry.</param>
        /// <param name="delay">The delay retries are done.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used to control the cancellation
        /// of the retry loop.
        /// </param>
        /// <param name="progress">
        /// The <see cref="IProgress{T}"/> to monitor the state of the retry loop.
        /// </param>
        public static async Task<T> RetryFuncAsync<T>(Func<Task<T>> func
            , TimeSpan delay
            , CancellationToken cancellationToken
            , IProgress<bool> progress = null)
        {
            T result = default;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(delay, cancellationToken);
                bool success = false;

                try
                {
                    var task = func();

                    if (task.Status == TaskStatus.Created)
                        task.Start();

                    result = await task;
                    success = true;
                }
                catch { }

                if (!(progress is null))
                    progress.Report(success);

                if (success)
                    break;
            }

            return result;
        }

        /// <summary>
        /// Retries an <see cref="Func{T, Task{TResult}}"/> asynchronously.
        /// </summary>
        /// <param name="func">
        /// The <see cref="Func{T, Task{TResult}}"/> to retry.
        /// </param>
        /// <param name="param">Parameter of the <see cref="Func{T, Task{TResult}}"/></param>
        /// <param name="delay">The delay retries are done.</param>
        /// <param name="cancellationToken">
        /// The <see cref="CancellationToken"/> used to control the cancellation
        /// of the retry loop.
        /// </param>
        /// <param name="progress">
        /// The <see cref="IProgress{T}"/> to monitor the state of the retry loop.
        /// </param>
        public static async Task<TResult> RetryFuncAsync<T, TResult>(Func<T, Task<TResult>> func
            , T param
            , TimeSpan delay
            , CancellationToken cancellationToken
            , IProgress<bool> progress = null)
        {
            TResult result = default;

            while (true)
            {
                if (cancellationToken.IsCancellationRequested)
                    break;

                await Task.Delay(delay, cancellationToken);
                bool success = false;

                try
                {
                    var task = func(param);

                    if (task.Status == TaskStatus.Created)
                        task.Start();

                    result = await task;
                    success = true;
                }
                catch { }

                if (!(progress is null))
                    progress.Report(success);

                if (success)
                    break;
            }

            return result;
        }

        #endregion Methods
    }
}