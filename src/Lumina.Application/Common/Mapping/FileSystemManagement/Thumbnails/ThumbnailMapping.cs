#region ========================================================================= USING =====================================================================================
using Lumina.Contracts.Responses.FileSystemManagement.Thumbnails;
using Lumina.Domain.Core.Aggregates.FileSystemManagement.FileSystemManagementAggregate.ValueObjects;
#endregion

namespace Lumina.Application.Common.Mapping.FileSystemManagement.Thumbnails;

/// <summary>
/// Extension methods for converting <see cref="Thumbnail"/>.
/// </summary>
public static class ThumbnailMapping
{
    /// <summary>
    /// Converts <paramref name="domainModel"/> to <see cref="ThumbnailResponse"/>.
    /// </summary>
    /// <param name="domainModel">The domain model to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ThumbnailResponse ToResponse(this Thumbnail domainModel)
    {
        return new ThumbnailResponse(
            domainModel.Type,
            domainModel.Bytes
        );
    }
}
