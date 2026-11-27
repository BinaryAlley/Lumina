#region ========================================================================= USING =====================================================================================
using Nemera.Domain.Common.Primitives;
using Nemera.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Nemera.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Nemera.Domain.SharedKernel.Common.Enums.MediaLibrary;
#endregion

namespace Nemera.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Value Object for the consumption rules of a photo.
/// </summary>
public sealed class PhotoConsumptionRule : EverySessionConsumptionRule
{
    /// <summary>
    /// Gets the kind of media that this rule applies to.
    /// </summary>
    public override MediaKind MediaKind { get; } = MediaKind.Photo;

    /// <summary>
    /// Gets a value indicating whether the current catalogues support this kind of media.
    /// </summary>
    public override bool IsSupportedByCurrentCatalogues { get; } = false;

    /// <summary>
    /// Gets the kind of position used to resume this kind of media. A photo cannot be resumed, so it has no position kind.
    /// </summary>
    public override Optional<MediaPositionKind> PositionKind { get; } = Optional<MediaPositionKind>.None();

    /// <summary>
    /// Gets a value indicating whether a consumption session of this kind of media is a one-shot session, which completes at the moment it starts.
    /// </summary>
    public override bool IsOneShot { get; } = true;
}
