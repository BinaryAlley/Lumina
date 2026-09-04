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
        /// <summary>
        /// Object used to synchronize the access to the runtime state of the scheduled job.
        /// </summary>
        public readonly object SyncRoot = new();

        /// <summary>
        /// Gets or sets the cancellation token source of the running execution cycle, or <see langword="null"/> when no cycle is running.
        /// </summary>
        public CancellationTokenSource? CycleCancellationTokenSource;

        /// <summary>
        /// Gets the slot that guards a scheduled job against overlapping executions.
        /// </summary>
        public readonly SemaphoreSlim RunSlot = new(1, 1);
    }

    // One runtime state instance exists per scheduled job, and it is shared by every thread that works on that job: the
    // cycle worker of the job, the request handlers that stop or remove the job, and the one time (fired) executions. The
    // ConcurrentDictionary only makes the lookup of the state instance itself thread safe; the fields of the instance are
    // still plain mutable fields, so every read or write of them is guarded by the per-state lock. The lock is only ever
    // held for the few instructions that mutate or read the state, never while a cancellation callback runs, and the lock
    // is reentrant for the thread that owns it, which keeps the registry free of deadlocks.
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
        lock (state.SyncRoot)
        {
            // The check and the assignment must be one atomic step: without the lock, two concurrent starts of the same job could both observe that
            // no cycle is running and both store their own cancellation token source, so a job would end up with two live cycles at once.
            // The lock serializes the starts, so exactly one wins.
            if (state.CycleCancellationTokenSource is not null)
                return false;
            state.CycleCancellationTokenSource = cycleCancellationTokenSource;
            return true;
        }
    }

    /// <summary>
    /// Gets a value indicating whether the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/> is currently running.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is checked.</param>
    /// <returns><see langword="true"/> when a cycle is currently running, <see langword="false"/> otherwise.</returns>
    public bool HasActiveCycle(ScheduledJobId scheduledJobId)
    {
        if (!_runtimeStates.TryGetValue(scheduledJobId, out ScheduledJobRuntimeState? state))
            return false;
        // The read takes the same lock the writers use, so it never observes a partially updated state, and the lock also acts as the memory barrier
        // that guarantees the read sees the latest value written by another thread.
        lock (state.SyncRoot)
            return state.CycleCancellationTokenSource is not null;
    }

    /// <summary>
    /// Stops the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is stopped.</param>
    public void StopCycle(ScheduledJobId scheduledJobId)
    {
        if (!_runtimeStates.TryGetValue(scheduledJobId, out ScheduledJobRuntimeState? state))
            return;
        CancellationTokenSource? cycleCancellationTokenSource;
        lock (state.SyncRoot)
        {
            // The stored token source is read and cleared while the lock is held, so that a start that races with this stop either sees the cycle and loses,
            // or sees no cycle and starts a fresh one whose token source is not the one cancelled here.
            cycleCancellationTokenSource = state.CycleCancellationTokenSource;
            state.CycleCancellationTokenSource = null;
        }
        // Cancelling a token source synchronously invokes every callback registered on its token, and those callbacks can reach back into this registry
        // (for example the cycle worker that unwinds and ends its own cycle). The lock is therefore released before the cancellation runs,
        // so no callback ever executes while the lock is held, and the lock is never held longer than the few instructions that mutate the state.
        cycleCancellationTokenSource?.Cancel();
    }

    /// <summary>
    /// Ends the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>, when it was started with <paramref name="cycleCancellationTokenSource"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is ended.</param>
    /// <param name="cycleCancellationTokenSource">The cancellation token source of the cycle that is ending.</param>
    /// <remarks>
    /// The cycle worker calls this method when its cycle finishes, so that a cycle that was replaced by a newer one
    /// after being stopped can never cancel the cycle that replaced it.
    /// </remarks>
    public void EndCycle(ScheduledJobId scheduledJobId, CancellationTokenSource cycleCancellationTokenSource)
    {
        if (_runtimeStates.TryGetValue(scheduledJobId, out ScheduledJobRuntimeState? state))
            lock (state.SyncRoot)
                // The identity check matters for the stop-then-start race: when a worker is stopped and a newer cycle is started before the old worker reaches
                // its cleanup, the stored token source already belongs to the new cycle. The old worker must only clear the field when it still owns it,
                // so it can never cancel the cycle that replaced it.
                if (ReferenceEquals(state.CycleCancellationTokenSource, cycleCancellationTokenSource))
                    state.CycleCancellationTokenSource = null;
        // Cancelling the worker's own token source is always safe and idempotent, and runs outside the lock for the same reason as in StopCycle:
        // the cancellation callbacks must not execute while the lock is held.
        cycleCancellationTokenSource.Cancel();
    }

    /// <summary>
    /// Attempts to reserve the execution slot of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose execution slot is reserved.</param>
    /// <returns><see langword="true"/> when the slot was reserved, <see langword="false"/> when another execution is already running.</returns>
    public bool TryAcquireRunSlot(ScheduledJobId scheduledJobId)
    {
        ScheduledJobRuntimeState state = _runtimeStates.GetOrAdd(scheduledJobId, static _ => new ScheduledJobRuntimeState());
        lock (state.SyncRoot)
            // The slot is acquired under the same lock that guards the rest of the state, so the acquire is serialized
            // with the cycle start and end transitions of the job; Wait(0) never blocks, so the lock is held only for a single non blocking instruction.
            return state.RunSlot.Wait(0);
    }

    /// <summary>
    /// Releases the execution slot of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose execution slot is released.</param>
    public void ReleaseRunSlot(ScheduledJobId scheduledJobId)
    {
        if (!_runtimeStates.TryGetValue(scheduledJobId, out ScheduledJobRuntimeState? state))
            return;
        lock (state.SyncRoot)
            // The semaphore is only released when a slot is actually taken, which is checked under the same lock as the acquire;
            // without that check, releasing a slot that was never acquired, or releasing it twice, would throw a SemaphoreFullException.
            if (state.RunSlot.CurrentCount is 0)
                state.RunSlot.Release();
    }
}
