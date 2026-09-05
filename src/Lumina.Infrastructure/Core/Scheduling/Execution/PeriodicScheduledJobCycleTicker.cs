#region ========================================================================= USING =====================================================================================
using System;
using System.Threading;
using System.Threading.Tasks;
#endregion

namespace Lumina.Infrastructure.Core.Scheduling.Execution;

/// <summary>
/// Default <see cref="IScheduledJobCycleTicker"/> implementation that ticks on a <see cref="PeriodicTimer"/>.
/// </summary>
internal sealed class PeriodicScheduledJobCycleTicker : IScheduledJobCycleTicker
{
    private readonly PeriodicTimer _periodicTimer;

    /// <summary>
    /// Initializes a new instance of the <see cref="PeriodicScheduledJobCycleTicker"/> class.
    /// </summary>
    /// <param name="period">The period between the ticks of the ticker.</param>
    public PeriodicScheduledJobCycleTicker(TimeSpan period)
    {
        _periodicTimer = new PeriodicTimer(period);
    }

    /// <summary>
    /// Sets the period between the ticks of the ticker.
    /// </summary>
    public TimeSpan Period
    {
        set
        {
            _periodicTimer.Period = value;
        }
    }

    /// <summary>
    /// Waits until the next tick of the ticker, or until the ticker is disposed or <paramref name="cancellationToken"/> is cancelled.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token that can be used to stop the wait.</param>
    /// <returns><see langword="true"/> when a tick occurred; <see langword="false"/> when the ticker was disposed before the next tick.</returns>
    public ValueTask<bool> WaitForNextTickAsync(CancellationToken cancellationToken)
    {
        return _periodicTimer.WaitForNextTickAsync(cancellationToken);
    }

    /// <summary>
    /// Disposes the periodic timer owned by the ticker.
    /// </summary>
    public void Dispose()
    {
        _periodicTimer.Dispose();
    }
}
