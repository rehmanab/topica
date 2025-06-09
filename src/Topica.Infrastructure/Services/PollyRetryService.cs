using Polly;
using Polly.Timeout;
using Topica.Infrastructure.Contracts;

namespace Topica.Infrastructure.Services;

public class PollyRetryService : IPollyRetryService
{
    public async Task<TResult?> WaitAndRetryAsync<TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<int> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<Task<TResult>> executeFunction,
        bool doInitialSleep)
    {
        if (doInitialSleep)
        {
            await Task.Delay(sleepDuration(0));
        }

        var retryPolicy = Policy
            .HandleResult(handleResultCondition)
            .WaitAndRetryAsync(retries, sleepDuration, (ex, ts, index, context) => { retryAction.Invoke(index); });

        try
        {
            var result = await retryPolicy.ExecuteAsync(executeFunction);

            return result;
        }
        catch
        {
            return default;
        }
    }

    public async Task WaitAndRetryAsync<TException>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<Exception, TimeSpan, int, Context> retryAction,
        Func<Task> executeFunction,
        bool doInitialSleep)
        where TException : Exception
    {
        if (doInitialSleep)
        {
            await Task.Delay(sleepDuration(0));
        }

        var retryPolicy = Policy
            .Handle<TException>()
            .WaitAndRetryAsync(retries, sleepDuration, retryAction.Invoke);

        await retryPolicy.ExecuteAsync(executeFunction);
    }

    public async Task WaitAndRetryAsync<TException1, TException2>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<Exception, TimeSpan, int, Context> retryAction,
        Func<Task> executeFunction,
        bool doInitialSleep)
        where TException1 : Exception
        where TException2 : Exception
    {
        if (doInitialSleep)
        {
            await Task.Delay(sleepDuration(0));
        }

        var retryPolicy = Policy
            .Handle<TException1>()
            .Or<TException2>()
            .WaitAndRetryAsync(retries, sleepDuration, retryAction.Invoke);

        await retryPolicy.ExecuteAsync(executeFunction);
    }

    public async Task<TResult> WaitAndRetryAsync<TException, TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<Exception, TimeSpan, int, Context> retryAction,
        Func<Task<TResult>> executeFunction)
        where TException : Exception
    {
        var retryPolicy = Policy
            .Handle<TException>()
            .WaitAndRetryAsync(retries, sleepDuration, retryAction.Invoke);

        return await retryPolicy.ExecuteAsync(executeFunction);
    }

    public async Task<TResult?> WaitAndRetryAsync<TException, TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<DelegateResult<TResult>, TimeSpan, int, Context> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<Task<TResult?>> executeFunction,
        bool doInitialSleep)
        where TException : Exception
    {
        if (doInitialSleep)
        {
            await Task.Delay(sleepDuration(0));
        }

        var retryPolicy = Policy
            .Handle<TException>()
            .OrResult(handleResultCondition)
            .WaitAndRetryAsync(retries, sleepDuration, retryAction.Invoke);

        try
        {
            var result = await retryPolicy.ExecuteAsync(executeFunction);

            return result;
        }
        catch
        {
            return default;
        }
    }

    public async Task<TResult?> WaitAndRetryAsync<TException1, TException2, TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<DelegateResult<TResult>, TimeSpan, int, Context> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<Task<TResult>> executeFunction,
        bool doInitialSleep)
        where TException1 : Exception
        where TException2 : Exception
    {
        if (doInitialSleep)
        {
            await Task.Delay(sleepDuration(0));
        }

        var retryPolicy = Policy
            .Handle<TException1>()
            .Or<TException2>()
            .OrResult(handleResultCondition)
            .WaitAndRetryAsync(retries, sleepDuration, retryAction.Invoke);

        try
        {
            var result = await retryPolicy.ExecuteAsync(executeFunction);

            return result;
        }
        catch
        {
            return default;
        }
    }

    public async Task<TResult?> WaitAndRetryWithTimeoutAsync<TException1, TException2, TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<DelegateResult<TResult>, TimeSpan, int, Context> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<Task<TResult>> executeFunction,
        bool doInitialSleep,
        TimeSpan timeout)
        where TException1 : Exception
        where TException2 : Exception
    {
        if (doInitialSleep)
        {
            await Task.Delay(sleepDuration(0));
        }

        var timeoutPolicy = Policy.TimeoutAsync<TResult>(timeout, TimeoutStrategy.Pessimistic,
            (context, timeSpan, task, ex) =>
            {
                Console.WriteLine($"Timeout policy applied: {timeout}");
                return Task.CompletedTask;
            });

        var retryPolicy = Policy
            .Handle<TException1>()
            .Or<TException2>()
            .OrResult(handleResultCondition)
            .WaitAndRetryAsync(retries, sleepDuration, retryAction.Invoke);

        try
        {
            var resilientStrategy = Policy.WrapAsync<TResult>(retryPolicy, timeoutPolicy);
            var result = await resilientStrategy.ExecuteAsync(() => executeFunction());

            return result;
        }
        catch
        {
            return default;
        }
    }

    public async Task<TResult?> WaitAndRetryWithTimeoutAsync<TException1, TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<DelegateResult<TResult>, TimeSpan, int, Context> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<Task<TResult>> executeFunction,
        bool doInitialSleep,
        TimeSpan timeout)
        where TException1 : Exception
    {
        if (doInitialSleep) await Task.Delay(sleepDuration(0));

        var timeoutPolicy = Policy.TimeoutAsync<TResult>(timeout, TimeoutStrategy.Pessimistic,
            (context, timeSpan, task, ex) =>
            {
                Console.WriteLine($"Timeout policy applied: {timeout}");
                return Task.CompletedTask;
            });

        var retryPolicy = Policy
            .Handle<TException1>()
            .OrResult(handleResultCondition)
            .WaitAndRetryAsync(retries, sleepDuration, retryAction.Invoke);

        try
        {
            var resilientStrategy = Policy.WrapAsync<TResult>(retryPolicy, timeoutPolicy);
            var result = await resilientStrategy.ExecuteAsync(() => executeFunction());

            return result;
        }
        catch
        {
            return default;
        }
    }

    public async Task<TResult?> WaitAndRetryForeverWithTimeoutAsync<TException1, TException2, TResult>(Func<int, TimeSpan> sleepDuration,
        Action<DelegateResult<TResult>, int, TimeSpan> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<Task<TResult>> executeFunction,
        bool doInitialSleep,
        TimeSpan timeout)
        where TException1 : Exception
        where TException2 : Exception
    {
        if (doInitialSleep) await Task.Delay(sleepDuration(0));

        var timeoutPolicy = Policy.TimeoutAsync<TResult>(timeout, TimeoutStrategy.Pessimistic, (context, timeSpan, task, ex) => Task.CompletedTask);

        var retryPolicy = Policy
            .Handle<TException1>()
            .Or<TException2>()
            .OrResult(handleResultCondition)
            .WaitAndRetryForeverAsync(sleepDuration, retryAction.Invoke);

        try
        {
            return await Policy.WrapAsync(retryPolicy, timeoutPolicy).ExecuteAsync(executeFunction);
        }
        catch
        {
            return default;
        }
    }
}