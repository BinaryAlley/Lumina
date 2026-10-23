#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

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
}
