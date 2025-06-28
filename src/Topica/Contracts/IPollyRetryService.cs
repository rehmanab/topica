using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Topica.Contracts;

public interface IPollyRetryService
{
    Task<TResult?> WaitAndRetryAsync<TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<int> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<CancellationToken, Task<TResult>> executeFunction,
        bool doInitialSleep,
        CancellationToken cancellationToken);

    Task WaitAndRetryAsync<TException>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<Exception, TimeSpan, int, Context> retryAction,
        Func<CancellationToken, Task> executeFunction,
        bool doInitialSleep,
        CancellationToken cancellationToken)
        where TException : Exception;
    
    Task WaitAndRetryAsync<TException1, TException2>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<Exception, TimeSpan, int, Context> retryAction,
        Func<CancellationToken, Task> executeFunction,
        bool doInitialSleep,
        CancellationToken cancellationToken)
        where TException1 : Exception
        where TException2 : Exception;

    Task<TResult> WaitAndRetryAsync<TException, TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<Exception, TimeSpan, int, Context> retryAction,
        Func<CancellationToken, Task<TResult>> executeFunction,
        CancellationToken cancellationToken)
        where TException : Exception;

    Task<TResult?> WaitAndRetryAsync<TException, TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<DelegateResult<TResult>, TimeSpan, int, Context> retryAction,
        Func<TResult?, bool> handleResultCondition,
        Func<CancellationToken, Task<TResult>> executeFunction,
        bool doInitialSleep,
        CancellationToken cancellationToken)
        where TException : Exception;

    Task<TResult?> WaitAndRetryAsync<TException1, TException2, TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<DelegateResult<TResult>, TimeSpan, int, Context> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<CancellationToken, Task<TResult>> executeFunction,
        bool doInitialSleep,
        CancellationToken cancellationToken)
        where TException1 : Exception
        where TException2 : Exception;

    Task<TResult?> WaitAndRetryWithTimeoutAsync<TException1, TException2, TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<DelegateResult<TResult>, TimeSpan, int, Context> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<CancellationToken, Task<TResult>> executeFunction,
        bool doInitialSleep,
        TimeSpan timeout,
        CancellationToken cancellationToken)
        where TException1 : Exception
        where TException2 : Exception;

    Task<TResult?> WaitAndRetryWithTimeoutAsync<TException1, TResult>(int retries,
        Func<int, TimeSpan> sleepDuration,
        Action<DelegateResult<TResult>, TimeSpan, int, Context> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<CancellationToken, Task<TResult>> executeFunction,
        bool doInitialSleep,
        TimeSpan timeout,
        CancellationToken cancellationToken)
        where TException1 : Exception;

    Task<TResult?> WaitAndRetryForeverWithTimeoutAsync<TException1, TException2, TResult>(Func<int, TimeSpan> sleepDuration,
        Action<DelegateResult<TResult>, int, TimeSpan> retryAction,
        Func<TResult, bool> handleResultCondition,
        Func<CancellationToken, Task<TResult>> executeFunction,
        bool doInitialSleep,
        TimeSpan timeout,
        CancellationToken cancellationToken)
        where TException1 : Exception
        where TException2 : Exception;
}