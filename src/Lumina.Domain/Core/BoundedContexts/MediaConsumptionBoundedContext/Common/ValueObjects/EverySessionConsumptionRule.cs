#region ========================================================================= USING =====================================================================================
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using System;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;

/// <summary>
/// Value Object for the consumption rules of a kind of media whose sessions count every time, regardless of their duration or progress. I.e.: photos.
/// </summary>
public abstract class EverySessionConsumptionRule : MediaConsumptionRule
{
    /// <summary>
    /// Determines whether a consumption session of this kind of media counts towards statistics.
    /// </summary>
    /// <param name="status">The status of the consumption session. It is ignored, because every session of this kind of media counts.</param>
    /// <param name="consumedDuration">The active duration consumed by the consumption session. It is ignored, because every session of this kind of media counts.</param>
    /// <param name="consecutivePagesRead">The number of consecutive pages read in the consumption session. It is ignored, because every session of this kind of media counts.</param>
    /// <returns><see langword="true"/>, because every session of this kind of media counts towards statistics.</returns>
    public override bool Counts(ConsumptionStatus status, TimeSpan consumedDuration, int consecutivePagesRead)
    {
        return true;
    }
}
