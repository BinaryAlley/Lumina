#region ========================================================================= USING =====================================================================================
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System;
using System.Linq;
#endregion

namespace Lumina.Application.Common.Mapping.MediaLibrary.Management;

/// <summary>
/// Extension methods for converting <see cref="Library"/>.
/// </summary>
public static class LibraryMapping
{
    /// <summary>
    /// Converts <paramref name="domainEntity"/> to <see cref="LibraryEntity"/>.
    /// </summary>
    /// <param name="domainEntity">The domain entity to be converted.</param>
    /// <returns>The converted repository entity.</returns>
    public static LibraryEntity ToRepositoryEntity(this Library domainEntity)
    {
        return new LibraryEntity
        {
            Id = domainEntity.Id.Value,
            Title = domainEntity.Title,
            LibraryType = domainEntity.LibraryType,
            UserId = domainEntity.UserId.Value,
            ContentLocations = domainEntity.ContentLocations.Select(path => new LibraryContentLocationEntity() { Path = path.Path }).ToList(),
            CoverImage = domainEntity.CoverImage,
            IsEnabled = domainEntity.IsEnabled,
            IsLocked = domainEntity.IsLocked,
            DownloadMedatadaFromWeb = domainEntity.DownloadMedatadaFromWeb,
            SaveMetadataInMediaDirectories = domainEntity.SaveMetadataInMediaDirectories,
            CreatedOnUtc = domainEntity.CreatedOnUtc,
            CreatedBy = Guid.Empty,
            UpdatedOnUtc = domainEntity.UpdatedOnUtc.HasValue ? domainEntity.UpdatedOnUtc : null,
            UpdatedBy = Guid.Empty
        };
    }
}
