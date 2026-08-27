#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryEntityFixture
{
    /// <summary>
    /// Creates a random valid <see cref="LibraryEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library.</param>
    /// <param name="userId">Optional. The Id of the user that owns the media library.</param>
    /// <param name="title">Optional. The title of the media library.</param>
    /// <param name="libraryType">Optional. The type of the media library.</param>
    /// <param name="contentLocations">Optional. The content locations of the media library.</param>
    /// <param name="coverImage">Optional. The cover image path of the media library.</param>
    /// <param name="isEnabled">Whether the media library is enabled or not.</param>
    /// <param name="isLocked">Whether the media library is locked or not.</param>
    /// <param name="canDownloadMetadataFromWeb">Whether the media library should download metadata from the web or not.</param>
    /// <param name="shouldSaveMetadataInMediaDirectories">Whether the metadata should be saved in the media directories or not.</param>
    /// <param name="shouldSkipUnchangedDirectoriesDuringScan">Whether unchanged directories should be skipped during a scan or not.</param>
    /// <param name="metadataProvidersConfigurationFingerprint">Optional. The fingerprint of the metadata provider configuration of the media library.</param>
    /// <param name="artworkProvidersConfigurationFingerprint">Optional. The fingerprint of the artwork provider configuration of the media library.</param>
    /// <returns>The created media library entity.</returns>
    public LibraryEntity Create(
        Guid? id = null,
        Guid? userId = null,
        string? title = null,
        LibraryType? libraryType = null,
        IEnumerable<string>? contentLocations = null,
        string? coverImage = null,
        bool isEnabled = true,
        bool isLocked = false,
        bool canDownloadMetadataFromWeb = true,
        bool shouldSaveMetadataInMediaDirectories = true,
        bool shouldSkipUnchangedDirectoriesDuringScan = false,
        string? metadataProvidersConfigurationFingerprint = null,
        string? artworkProvidersConfigurationFingerprint = null)
    {
        return new Faker<LibraryEntity>()
            .CustomInstantiator(f => new LibraryEntity
            {
                Id = id ?? Guid.NewGuid(),
                UserId = userId ?? Guid.NewGuid(),
                Title = default!,
                CoverImage = default,
                LibraryType = default,
                ContentLocations = [],
                LibraryScans = [],
                IsEnabled = isEnabled,
                IsLocked = isLocked,
                CanDownloadMetadataFromWeb = canDownloadMetadataFromWeb,
                ShouldSaveMetadataInMediaDirectories = shouldSaveMetadataInMediaDirectories,
                ShouldSkipUnchangedDirectoriesDuringScan = shouldSkipUnchangedDirectoriesDuringScan,
                MetadataProvidersConfigurationFingerprint = metadataProvidersConfigurationFingerprint,
                ArtworkProvidersConfigurationFingerprint = artworkProvidersConfigurationFingerprint,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedOnUtc = null,
                UpdatedBy = null
            })
            .RuleFor(library => library.Title, f => title ?? f.Lorem.Word())
            .RuleFor(library => library.LibraryType, f => libraryType ?? f.PickRandom<LibraryType>())
            .RuleFor(library => library.CoverImage, f => coverImage ?? f.System.FilePath())
            .RuleFor(library => library.ContentLocations, f =>
            {
                IEnumerable<string> paths = contentLocations ?? [f.System.DirectoryPath(), f.System.DirectoryPath()];
                return [.. paths.Select(path => new LibraryContentLocationEntity() { Path = path })];
            })
            .RuleFor(library => library.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(library => library.CreatedBy, f => f.Random.Guid())
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="LibraryEntity"/> instances.</returns>
    public List<LibraryEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
