#region ========================================================================= USING =====================================================================================
using Lumina.Domain.Common.Models.Core;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using System.Collections.Generic;
using System.Diagnostics;
#endregion

namespace Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;

/// <summary>
/// Value Object for the composite unique identifier of a media library scan.
/// </summary>
[DebuggerDisplay("ScanId: {ScanId.Value}; UserId: {UserId.Value}")]
public sealed class MediaLibraryScanCompositeId : ValueObject
{
    /// <summary>
    /// Gets the object representing the unique identifier of the media library scan to be tracked.
    /// </summary>
    public ScanId ScanId { get; }

    /// <summary>
    /// Gets the object representing the unique identifier of the username who initiated the media library scan.
    /// </summary>
    public UserId UserId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MediaLibraryScanCompositeId"/> class.
    /// </summary>
    /// <param name="scanId">The object representing the unique identifier of the media library scan to be tracked.</param>
    /// <param name="userId">The object representing the unique identifier of the username who initiated the media library scan.</param>
    private MediaLibraryScanCompositeId(ScanId scanId, UserId userId) 
    {
        ScanId = scanId;
        UserId = userId;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="MediaLibraryScanCompositeId"/> class.
    /// </summary>
    /// <param name="scanId">The object representing the unique identifier of the media library scan to be tracked.</param>
    /// <param name="userId">The object representing the unique identifier of the username who initiated the media library scan.</param>
    /// <returns>The created <see cref="MediaLibraryScanCompositeId"/> instance.</returns>
    public static MediaLibraryScanCompositeId Create(ScanId scanId, UserId userId)
    {
        return new MediaLibraryScanCompositeId(scanId, userId);
    }

    /// <inheritdoc/>
    public override IEnumerable<object> GetEqualityComponents()
    {
        yield return ScanId;
        yield return UserId;
    }

    /// <summary>
    /// Customized ToString() method.
    /// </summary>
    /// <returns>Custom string value showing relevant data for current class.</returns>
    public override string ToString()
    {
        return $"{ScanId}-{UserId}";
    }
}
