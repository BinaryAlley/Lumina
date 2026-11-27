#region ========================================================================= USING =====================================================================================
using Nemera.Contracts.Responses.FileSystemManagement.Thumbnails;
using Nemera.Domain.Core.BoundedContexts.FileSystemManagementBoundedContext.FileSystemManagementAggregate.ValueObjects;
#endregion

namespace Nemera.Application.Common.Mapping.FileSystemManagement.Thumbnails;

/// <summary>
/// Extension methods for converting <see cref="Thumbnail"/>.
/// </summary>
public static class ThumbnailMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="ThumbnailResponse"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted response.</returns>
    public static ThumbnailResponse ToResponse(this Thumbnail domainEntity)
    {
        return new ThumbnailResponse(
            domainEntity.Type,
            domainEntity.Bytes
        );
    }
}
