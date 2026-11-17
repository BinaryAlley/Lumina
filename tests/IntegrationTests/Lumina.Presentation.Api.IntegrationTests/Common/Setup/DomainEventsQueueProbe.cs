#region ========================================================================= USING =====================================================================================
using System;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Controls and observes the domain events handled through the <see cref="ProbingDomainEventsQueue"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class DomainEventsQueueProbe
{
    private readonly Lock _signalLock = new();
    private readonly ConcurrentDictionary<string, int> _dequeueAttemptsByPath = new();
    private readonly ConcurrentDictionary<string, TaskCompletionSource<bool>> _dequeueSignalsByPath = new();
    private TaskCompletionSource<bool> _enqueuedSignal = CreateSignal();
    private TaskCompletionSource<bool> _enqueueReleaseSignal = CreateSignal();
    private volatile bool _shouldBlockOnEnqueue;
    private volatile bool _shouldThrowOnEnqueue;
    private int _enqueueCount;
    private int _dequeueCount;

    /// <summary>
    /// Gets or sets whether enqueuing a domain event waits until the test releases it.
    /// </summary>
    public bool ShouldBlockOnEnqueue
    {
        get { return _shouldBlockOnEnqueue; }
        set { _shouldBlockOnEnqueue = value; }
    }

    /// <summary>
    /// Gets or sets whether enqueuing a domain event fails after the event is queued.
    /// </summary>
    public bool ShouldThrowOnEnqueue
    {
        get { return _shouldThrowOnEnqueue; }
        set { _shouldThrowOnEnqueue = value; }
    }

    /// <summary>
    /// Gets or sets how long a blocked enqueuing request waits for the test to release it.
    /// </summary>
    public TimeSpan BlockTimeout { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the number of domain events that were enqueued.
    /// </summary>
    public int EnqueueCount => Volatile.Read(ref _enqueueCount);

    /// <summary>
    /// Gets the number of domain events that were dequeued for publication.
    /// </summary>
    public int DequeueCount => Volatile.Read(ref _dequeueCount);

    /// <summary>
    /// Clears the recorded counts and signals, so that a test observes only the events handled after the reset.
    /// </summary>
    public void Reset()
    {
        ShouldBlockOnEnqueue = false;
        ShouldThrowOnEnqueue = false;
        Interlocked.Exchange(ref _enqueueCount, 0);
        Interlocked.Exchange(ref _dequeueCount, 0);
        _dequeueAttemptsByPath.Clear();
        _dequeueSignalsByPath.Clear();
        lock (_signalLock)
        {
            _enqueuedSignal = CreateSignal();
            _enqueueReleaseSignal = CreateSignal();
        }
    }

    /// <summary>
    /// Records that a domain event was enqueued, optionally blocking or failing the enqueuing request.
    /// </summary>
    public void RecordEnqueue()
    {
        Interlocked.Increment(ref _enqueueCount);
        TaskCompletionSource<bool> enqueuedSignal;
        TaskCompletionSource<bool> releaseSignal;
        lock (_signalLock)
        {
            enqueuedSignal = _enqueuedSignal;
            releaseSignal = _enqueueReleaseSignal;
        }
        enqueuedSignal.TrySetResult(true);
        if (ShouldBlockOnEnqueue && !releaseSignal.Task.Wait(BlockTimeout))
            throw new TimeoutException("The test did not release the queued domain event in time.");
        if (ShouldThrowOnEnqueue)
            throw new InvalidOperationException("The test forces the request to fail after its domain event was queued.");
    }

    /// <summary>
    /// Records that the queue was asked for a domain event on the request of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path of the request whose middleware dequeued the queue.</param>
    /// <param name="wasDequeued">Whether the queue actually contained an event.</param>
    public void RecordDequeue(string path, bool wasDequeued)
    {
        _dequeueAttemptsByPath.AddOrUpdate(path, 1, (_, attemptCount) => attemptCount + 1);
        TaskCompletionSource<bool> signal = _dequeueSignalsByPath.GetOrAdd(path, _ => CreateSignal());
        signal.TrySetResult(true);
        if (wasDequeued)
            Interlocked.Increment(ref _dequeueCount);
    }

    /// <summary>
    /// Gets the number of dequeue attempts made for the request of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path of the request.</param>
    /// <returns>The number of dequeue attempts made for the request.</returns>
    public int GetDequeueAttemptCount(string path)
    {
        return _dequeueAttemptsByPath.TryGetValue(path, out int attemptCount) ? attemptCount : 0;
    }

    /// <summary>
    /// Waits until a dequeue attempt is made for the request of <paramref name="path"/>.
    /// </summary>
    /// <param name="path">The path of the request to observe.</param>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task WaitForDequeueAsync(string path, TimeSpan timeout)
    {
        Task signal = _dequeueSignalsByPath.GetOrAdd(path, _ => CreateSignal()).Task;
        Task completedTask = await Task.WhenAny(signal, Task.Delay(timeout)).ConfigureAwait(false);
        if (completedTask != signal)
            throw new TimeoutException($"The domain events queue was not dequeued for '{path}' within {timeout}.");
    }

    /// <summary>
    /// Waits until a domain event is enqueued.
    /// </summary>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task WaitForEnqueueAsync(TimeSpan timeout)
    {
        Task signal;
        lock (_signalLock)
            signal = _enqueuedSignal.Task;
        Task completedTask = await Task.WhenAny(signal, Task.Delay(timeout)).ConfigureAwait(false);
        if (completedTask != signal)
            throw new TimeoutException($"A domain event was not enqueued within {timeout}.");
    }

    /// <summary>
    /// Releases an enqueuing request blocked by <see cref="ShouldBlockOnEnqueue"/>.
    /// </summary>
    public void ReleaseEnqueue()
    {
        TaskCompletionSource<bool> releaseSignal;
        lock (_signalLock)
            releaseSignal = _enqueueReleaseSignal;
        releaseSignal.TrySetResult(true);
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
