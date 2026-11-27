#region ========================================================================= USING =====================================================================================
using Nemera.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Nemera.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using System.Linq;
#endregion

namespace Nemera.Application.Common.Mapping.MediaLibrary.Management;

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
            ContentLocations = [.. domainEntity.ContentLocations.Select(path => new LibraryContentLocationEntity() { Path = path.Path })],
            PathTemplateParts = [.. domainEntity.PathTemplate.Parts.Select((part, index) => new LibraryPathTemplatePartEntity
            {
                Kind = part.Kind,
                Representation = part.Representation,
                IsOptional = part.IsOptional,
                Position = index
            })],
            CoverImage = domainEntity.CoverImage.HasValue ? domainEntity.CoverImage.Value : null,
            IsEnabled = domainEntity.IsEnabled,
            IsLocked = domainEntity.IsLocked,
            CanDownloadMetadataFromWeb = domainEntity.CanDownloadMetadataFromWeb,
            ShouldSaveMetadataInMediaDirectories = domainEntity.ShouldSaveMetadataInMediaDirectories,
            ShouldSkipUnchangedDirectoriesDuringScan = domainEntity.ShouldSkipUnchangedDirectoriesDuringScan,
            ShouldSkipHiddenFilesAndDirectoriesDuringScan = domainEntity.ShouldSkipHiddenFilesAndDirectoriesDuringScan,
            IsContentOnSsd = domainEntity.IsContentOnSsd,
            ShouldTrackConsumptionProgress = domainEntity.ConsumptionSettings.ShouldTrackConsumptionProgress,
            CompletionThresholdPercentage = domainEntity.ConsumptionSettings.CompletionThresholdPercentage.HasValue
                ? domainEntity.ConsumptionSettings.CompletionThresholdPercentage.Value
                : null,
            LibraryScans = [.. domainEntity.ScanIds.Select(scanId => new LibraryScanEntity()
            {
                Id = scanId.Value,
                LibraryId = domainEntity.Id.Value,
                UserId = domainEntity.UserId.Value, // Rest of properties will be populated by EF Core.
                Status = default,
                User = null!,
                Library = null!
            })]
        };
    }
}
