#region ========================================================================= USING =====================================================================================
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution;

/// <summary>
/// Represents a cancellable periodic ticker that drives the cadence of an execution cycle of a scheduled job.
/// </summary>
/// <remarks>
/// The interface is separated from the periodic timer it wraps so that the unit tests of the cycle worker can drive
/// the cadence with a stubbed ticker, instead of waiting on the real periodic timer, whose periods start at one minute.
/// </remarks>
internal interface IScheduledJobCycleTicker : IDisposable
{
    /// <summary>
    /// Sets the period between the ticks of the ticker.
    /// </summary>
    /// <remarks>
    /// The period of a daily schedule is recalculated after every run from the current time, so it is assigned after each tick.
    /// </remarks>
    TimeSpan Period { set; }

    /// <summary>
    /// Waits until the next tick of the ticker, or until the ticker is disposed or <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the wait.</param>
    /// <returns><see langword="true"/> when a tick occurred; <see langword="false"/> when the ticker was disposed before the next tick.</returns>
    ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken);
}
