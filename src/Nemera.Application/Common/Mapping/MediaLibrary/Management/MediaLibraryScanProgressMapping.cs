#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.Responses.MediaLibrary.Management;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
#endregion

namespace Nemera.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="MediaLibraryScanProgress"/>.
/// </summary>
public static class MediaLibraryScanProgressMapping
{
    /// <summary>
    /// Converts <paramref name="domainValueObject"/> to <see cref="MediaLibraryScanProgressResponse"/>.
    /// </summary>
    /// <param name="domainValueObject">The domain value object to be converted.</param>
    /// <returns>The converted response.</returns>
    public static MediaLibraryScanProgressResponse ToResponse(this MediaLibraryScanProgress domainValueObject)
    {
        return new MediaLibraryScanProgressResponse(
            domainValueObject.ScanId.Value,
            domainValueObject.UserId.Value,
            domainValueObject.LibraryId.Value,
            domainValueObject.TotalJobs,
            domainValueObject.CompletedJobs,
            domainValueObject.CurrentJobProgress.Value?.ToResponse(),
            domainValueObject.Status.ToString(),
            domainValueObject.OverallProgressPercentage
        );
    }
}
