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
    /// Stops the execution cycle of the scheduled job identified by <paramref name="scheduledJobId"/>.
    /// </summary>
    /// <param name="scheduledJobId">The object representing the unique identifier of the scheduled job whose cycle is stopped.</param>
    void StopCycle(ScheduledJobId scheduledJobId);

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
