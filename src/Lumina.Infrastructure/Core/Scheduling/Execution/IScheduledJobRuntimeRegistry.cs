#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Core.BoundedContexts.SchedulingBoundedContext.ScheduledJobAggregate.ValueObjects;
using System.Threading;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution;

/// <summary>
/// Interface for the registry that holds the live runtime state of the scheduled jobs.
/// </summary>
public interface IScheduledJobRuntimeRegistry
{
    /// <summary>
    /// Starts the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is started.</param>
    /// <param name="cycleCancellationTokenSource">The cancellation token source used to stop the execution cycle.</param>
    /// <returns><see langword="true"/> when the cycle was started, <see langword="false"/> when a cycle was already running.</returns>
    bool TryStartCycle(ScheduledJobId scheduledJobId, CancellationTokenSource cycleCancellationTokenSource);

    /// <summary>
    /// Gets a value indicating whether the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/> is currently running.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is checked.</param>
    /// <returns><see langword="true"/> when a cycle is currently running, <see langword="false"/> otherwise.</returns>
    bool HasActiveCycle(ScheduledJobId scheduledJobId);

    /// <summary>
    /// Stops the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is stopped.</param>
    void StopCycle(ScheduledJobId scheduledJobId);

    /// <summary>
    /// Ends the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>, when it was started with <paramref name="cycleCancellationTokenSource"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is ended.</param>
    /// <param name="cycleCancellationTokenSource">The cancellation token source of the cycle that is ending.</param>
    void EndCycle(ScheduledJobId scheduledJobId, CancellationTokenSource cycleCancellationTokenSource);

    /// <summary>
    /// Attempts to reserve the execution slot of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose execution slot is reserved.</param>
    /// <returns><see langword="true"/> when the slot was reserved, <see langword="false"/> when another execution is already running.</returns>
    bool TryAcquireRunSlot(ScheduledJobId scheduledJobId);

    /// <summary>
    /// Releases the execution slot of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose execution slot is released.</param>
    void ReleaseRunSlot(ScheduledJobId scheduledJobId);
}
