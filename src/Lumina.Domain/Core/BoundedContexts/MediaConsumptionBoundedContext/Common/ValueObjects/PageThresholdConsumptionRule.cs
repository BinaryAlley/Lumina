#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;

/// <summary>
/// Value Object for the consumption rules of a kind of media whose sessions count when they are completed or reach a minimum number of consecutive pages read. I.e.: books.
/// </summary>
public abstract class PageThresholdConsumptionRule : MediaConsumptionRule
{
    /// <summary>
    /// Gets the minimum number of consecutive pages that a session of this kind of media must read to be counted, when it is not completed.
    /// </summary>
    public abstract int MinimumConsecutivePages { get; }

    /// <summary>
    /// Determines whether a consumption session of this kind of media counts towards statistics.
    /// </summary>
    /// <param name="status">The status of the consumption session.</param>
    /// <param name="consumedDuration">The active duration consumed by the consumption session. It is ignored, because this kind of media is counted by pages.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the consumption session.</param>
    /// <returns><see langword="true"/> if the consumption session counts towards statistics, <see langword="false"/> otherwise.</returns>
    public override bool Counts(ConsumptionStatus status, TimeSpan consumedDuration, int consecutivePagesRead)
    {
        return status == ConsumptionStatus.Completed || consecutivePagesRead > MinimumConsecutivePages;
    }
}
