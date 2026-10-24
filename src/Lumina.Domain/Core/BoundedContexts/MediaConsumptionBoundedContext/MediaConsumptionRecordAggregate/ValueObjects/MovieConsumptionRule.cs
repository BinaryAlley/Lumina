#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Value Object for the consumption rules of a movie.
/// </summary>
public sealed class MovieConsumptionRule : DurationThresholdConsumptionRule
{
    /// <summary>
    /// Gets the kind of media that this rule applies to.
    /// </summary>
    public override MediaKind MediaKind { get; } = MediaKind.Movie;

    /// <summary>
    /// Gets a value indicating whether the current catalogues support this kind of media.
    /// </summary>
    public override bool IsSupportedByCurrentCatalogues { get; } = false;

    /// <summary>
    /// Gets the kind of position used to resume this kind of media, if the kind can be resumed.
    /// </summary>
    public override Optional<MediaPositionKind> PositionKind { get; } = MediaPositionKind.TimeOffset;

    /// <summary>
    /// Gets a value indicating whether a consumption session of this kind of media is a one-shot session, which completes at the moment it starts.
    /// </summary>
    public override bool IsOneShot { get; } = false;

    /// <summary>
    /// Gets the minimum consumed duration that a session of this kind of media must reach to be counted, when it is not completed.
    /// </summary>
    public override TimeSpan MinimumCountedDuration { get; } = TimeSpan.FromMinutes(3);
}
