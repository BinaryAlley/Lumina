#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;

/// <summary>
/// Value Object for the consumption rules of a kind of media whose sessions count when they are completed or reach a minimum consumed duration. I.e.: movies.
/// </summary>
public abstract class DurationThresholdConsumptionRule : MediaConsumptionRule
{
    /// <summary>
    /// Gets the minimum consumed duration that a session of this kind of media must reach to be counted, when it is not completed.
    /// </summary>
    public abstract TimeSpan MinimumCountedDuration { get; }

    /// <summary>
    /// Determines whether a consumption session of this kind of media counts towards statistics.
    /// </summary>
    /// <param name="status">The status of the consumption session.</param>
    /// <param name="consumedDuration">The active duration consumed by the consumption session.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the consumption session. It is ignored, because this kind of media is counted by duration.</param>
    /// <returns><see langword="true"/> if the consumption session counts towards statistics, <see langword="false"/> otherwise.</returns>
    public override bool Counts(ConsumptionStatus status, TimeSpan consumedDuration, int consecutivePagesRead)
    {
        return status == ConsumptionStatus.Completed || consumedDuration >= MinimumCountedDuration;
    }
}
