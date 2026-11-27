#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Observes the database transactions rolled back on each request through the <see cref="TransactionProbeInterceptor"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class TransactionProbe
{
    private readonly Lock _signalLock = new();
    private readonly Dictionary<string, TaskCompletionSource<bool>> _rollbackSignalsByPath = [];

    /// <summary>
    /// Clears the recorded signals, so that a test observes only the rollbacks that happen after the reset.
    /// </summary>
    public void Reset()
    {
        lock (_signalLock)
            _rollbackSignalsByPath.Clear();
    }

    /// <summary>
    /// Records that the transaction of the request of <paramref name="path"/> was rolled back.
    /// </summary>
    /// <param name="path">The path of the request whose transaction was rolled back.</param>
    public void RecordRolledBack(string path)
    {
        TaskCompletionSource<bool> signal;
        lock (_signalLock)
        {
            if (!_rollbackSignalsByPath.TryGetValue(path, out signal!))
            {
                signal = CreateSignal();
                _rollbackSignalsByPath[path] = signal;
            }
        }
        signal.TrySetResult(true);
    }

    /// <summary>
    /// Waits until the transaction of the request of <paramref name="path"/> is rolled back.
    /// </summary>
    /// <param name="path">The path of the request to observe.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task WaitForRollbackAsync(string path, TimeSpan timeout)
    {
        Task signal;
        lock (_signalLock)
        {
            if (!_rollbackSignalsByPath.TryGetValue(path, out TaskCompletionSource<bool>? signalSource))
            {
                signalSource = CreateSignal();
                _rollbackSignalsByPath[path] = signalSource;
            }
            signal = signalSource.Task;
        }
        Task completedTask = await Task.WhenAny(signal, Task.Delay(timeout)).ConfigureAwait(false);
        if (completedTask != signal)
            throw new TimeoutException($"The transaction of '{path}' was not rolled back within {timeout}.");
    }

    /// <summary>
    /// Creates a completion source that resumes its continuations asynchronously.
    /// </summary>
    /// <returns>The created completion source.</returns>
    private static TaskCompletionSource<bool> CreateSignal()
    {
        return new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    }
}
