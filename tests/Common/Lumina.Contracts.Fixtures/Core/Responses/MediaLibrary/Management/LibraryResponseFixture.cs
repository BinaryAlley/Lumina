#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.MediaLibrary.Management;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="LibraryResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="LibraryResponse"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the media library.</param>
    /// <param name="userId">Optional. The Id of the user owning the media library.</param>
    /// <param name="title">Optional. The title of the media library.</param>
    /// <param name="libraryType">Optional. The type of the media library.</param>
    /// <param name="contentLocations">Optional. The file system paths of the media library content directories.</param>
    /// <param name="coverImage">Optional. The path of the image file used as the cover of the library.</param>
    /// <param name="isEnabled">Optional. Whether the media library is enabled.</param>
    /// <param name="isLocked">Optional. Whether the media library is locked.</param>
    /// <param name="canDownloadMetadataFromWeb">Optional. Whether metadata should be downloaded from the web.</param>
    /// <param name="shouldSaveMetadataInMediaDirectories">Optional. Whether metadata should be saved in the media directories.</param>
    /// <param name="shouldSkipUnchangedDirectoriesDuringScan">Optional. Whether unchanged directories should be skipped during scan.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the library was created.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the library was updated.</param>
    /// <returns>The created <see cref="LibraryResponse"/>.</returns>
    public LibraryResponse Create(
        Guid? id = null,
        Guid? userId = null,
        string? title = null,
        LibraryType? libraryType = null,
        List<string>? contentLocations = null,
        string? coverImage = null,
        bool? isEnabled = null,
        bool? isLocked = null,
        bool? canDownloadMetadataFromWeb = null,
        bool? shouldSaveMetadataInMediaDirectories = null,
        bool? shouldSkipUnchangedDirectoriesDuringScan = null,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null)
    {
        return new LibraryResponse(
            id ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            title ?? _faker.Commerce.Department(),
            libraryType ?? _faker.PickRandom<LibraryType>(),
            contentLocations ?? [_faker.System.DirectoryPath(), _faker.System.DirectoryPath()],
            coverImage,
            isEnabled ?? _faker.Random.Bool(),
            isLocked ?? _faker.Random.Bool(),
            canDownloadMetadataFromWeb ?? _faker.Random.Bool(),
            shouldSaveMetadataInMediaDirectories ?? _faker.Random.Bool(),
            shouldSkipUnchangedDirectoriesDuringScan ?? _faker.Random.Bool(),
            createdOnUtc ?? _faker.Date.Past().ToUniversalTime(),
            updatedOnUtc
        );
    }

    /// <summary>
    /// Creates a list of <see cref="LibraryResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<LibraryResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
