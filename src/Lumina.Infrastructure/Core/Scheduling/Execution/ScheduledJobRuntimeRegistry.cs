#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System.Collections.Concurrent;
using System.Threading;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution;

/// <summary>
/// Registry that holds the live runtime state of the scheduled jobs.
/// </summary>
public sealed class ScheduledJobRuntimeRegistry : IScheduledJobRuntimeRegistry
{
    /// <summary>
    /// Class that holds the live runtime state of the execution cycle of a scheduled job.
    /// </summary>
    private sealed class ScheduledJobRuntimeState
    {
        public CancellationTokenSource? CycleCancellationTokenSource;
        public readonly SemaphoreSlim RunSlot = new(1, 1);
    }

    private readonly ConcurrentDictionary<ScheduledJobId, ScheduledJobRuntimeState> _runtimeStates = [];

    /// <summary>
    /// Starts the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is started.</param>
    /// <param name="cycleCancellationTokenSource">The cancellation token source used to stop the execution cycle.</param>
    /// <returns><see langword="true"/> when the cycle was started, <see langword="false"/> when a cycle was already running.</returns>
    public bool TryStartCycle(ScheduledJobId scheduledJobId, CancellationTokenSource cycleCancellationTokenSource)
    {
        ScheduledJobRuntimeState state = _runtimeStates.GetOrAdd(scheduledJobId, static _ => new ScheduledJobRuntimeState());
        if (state.CycleCancellationTokenSource is not null)
            return false;
        state.CycleCancellationTokenSource = cycleCancellationTokenSource;
        return true;
    }

    /// <summary>
    /// Stops the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is stopped.</param>
    public void StopCycle(ScheduledJobId scheduledJobId)
    {
        if (!_runtimeStates.TryGetValue(scheduledJobId, out ScheduledJobRuntimeState? state))
            return;
        CancellationTokenSource? cycleCancellationTokenSource = state.CycleCancellationTokenSource;
        state.CycleCancellationTokenSource = null;
        cycleCancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Attempts to reserve the execution slot of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose execution slot is reserved.</param>
    /// <returns><see langword="true"/> when the slot was reserved, <see langword="false"/> when another execution is already running.</returns>
    public bool TryAcquireRunSlot(ScheduledJobId scheduledJobId)
    {
        ScheduledJobRuntimeState state = _runtimeStates.GetOrAdd(scheduledJobId, static _ => new ScheduledJobRuntimeState());
        return state.RunSlot.Wait(0);
    }

    /// <summary>
    /// Releases the execution slot of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose execution slot is released.</param>
    public void ReleaseRunSlot(ScheduledJobId scheduledJobId)
    {
        if (_runtimeStates.TryGetValue(scheduledJobId, out ScheduledJobRuntimeState? state))
            state.RunSlot.Release();
    }
}
