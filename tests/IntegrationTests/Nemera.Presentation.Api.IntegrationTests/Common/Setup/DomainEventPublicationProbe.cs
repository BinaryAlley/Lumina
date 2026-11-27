#region ========================================================================= USING =====================================================================================
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Nemera.Presentation.Api.IntegrationTests.Common.Setup;

/// <summary>
/// Observes the domain events published through the real domain event publisher.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class DomainEventPublicationProbe
{
    private readonly Lock _signalLock = new();
    private TaskCompletionSource<bool> _publishedSignal = CreateSignal();
    private int _publicationCount;

    /// <summary>
    /// Gets the number of domain events that were published.
    /// </summary>
    public int PublicationCount => Volatile.Read(ref _publicationCount);

    /// <summary>
    /// Gets whether the favorite of the last published event was visible from a separate database scope.
    /// </summary>
    public bool WasFavoriteVisible { get; private set; }

    /// <summary>
    /// Clears the recorded observations, so that a test observes only the events published after the reset.
    /// </summary>
    public void Reset()
    {
        Interlocked.Exchange(ref _publicationCount, 0);
        WasFavoriteVisible = false;
        lock (_signalLock)
            _publishedSignal = CreateSignal();
    }

    /// <summary>
    /// Records that a domain event was published, together with whether its favorite was visible at publication time.
    /// </summary>
    /// <param name="wasFavoriteVisible">Whether the favorite was visible from a separate database scope.</param>
    public void RecordPublication(bool wasFavoriteVisible)
    {
        WasFavoriteVisible = wasFavoriteVisible;
        Interlocked.Increment(ref _publicationCount);
        TaskCompletionSource<bool> signal;
        lock (_signalLock)
            signal = _publishedSignal;
        signal.TrySetResult(true);
    }

    /// <summary>
    /// Waits until a domain event is published.
    /// </summary>
    /// <param name="timeout">The maximum time to wait.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task WaitForPublicationAsync(TimeSpan timeout)
    {
        Task signal;
        lock (_signalLock)
            signal = _publishedSignal.Task;
        Task completedTask = await Task.WhenAny(signal, Task.Delay(timeout)).ConfigureAwait(false);
        if (completedTask != signal)
            throw new TimeoutException($"A domain event was not published within {timeout}.");
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
