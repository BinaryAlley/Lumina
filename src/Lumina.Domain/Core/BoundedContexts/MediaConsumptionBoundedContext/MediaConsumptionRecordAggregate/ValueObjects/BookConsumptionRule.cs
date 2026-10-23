#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.Common.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaConsumption;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.MediaConsumptionBoundedContext.MediaConsumptionRecordAggregate.ValueObjects;

/// <summary>
/// Value Object for the consumption rules of a book.
/// </summary>
public sealed class BookConsumptionRule : PageThresholdConsumptionRule
{
    /// <summary>
    /// Gets the kind of media that this rule applies to.
    /// </summary>
    public override MediaKind MediaKind { get; } = MediaKind.Book;

    /// <summary>
    /// Gets a value indicating whether the current catalogues support this kind of media.
    /// </summary>
    public override bool IsSupportedByCurrentCatalogues { get; } = true;

    /// <summary>
    /// Gets the kind of position used to resume this kind of media, if the kind can be resumed.
    /// </summary>
    public override Optional<MediaPositionKind> PositionKind { get; } = MediaPositionKind.Location;

    /// <summary>
    /// Gets the minimum number of consecutive pages that a session of this kind of media must read to be counted, when it is not completed.
    /// </summary>
    public override int MinimumConsecutivePages { get; } = 5;
}
