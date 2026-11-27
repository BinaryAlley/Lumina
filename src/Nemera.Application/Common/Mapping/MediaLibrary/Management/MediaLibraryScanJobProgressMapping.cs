#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.Responses.MediaLibrary.Management;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
#endregion

namespace Nemera.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="MediaLibraryScanJobProgress"/>.
/// </summary>
public static class MediaLibraryScanJobProgressMapping
{
    /// <summary>
    /// Converts <paramref name="domainValueObject"/> to <see cref="MediaLibraryScanJobProgressResponse"/>.
    /// </summary>
    /// <param name="domainValueObject">The domain value object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static MediaLibraryScanJobProgressResponse ToResponse(this MediaLibraryScanJobProgress domainValueObject)
    {
        return new MediaLibraryScanJobProgressResponse(
            domainValueObject.CompletedItems,
            domainValueObject.TotalItems,
            domainValueObject.CurrentOperation,
            domainValueObject.ProgressPercentage
        );
    }
}
