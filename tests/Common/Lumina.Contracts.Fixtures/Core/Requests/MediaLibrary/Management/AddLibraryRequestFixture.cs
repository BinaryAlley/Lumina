#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.MediaLibrary.Management;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.Management;

/// <summary>
/// Fixture class for the <see cref="AddLibraryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class AddLibraryRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="AddLibraryRequest"/>.
    /// </summary>
    /// <param name="title">Optional. The title of the media library.</param>
    /// <param name="libraryType">Optional. The type of the media library.</param>
    /// <param name="contentLocations">Optional. The file system paths of the media library content directories.</param>
    /// <param name="coverImage">Optional. The path of the image file used as the cover.</param>
    /// <param name="isEnabled">Optional. Whether the media library is enabled.</param>
    /// <param name="isLocked">Optional. Whether the media library is locked.</param>
    /// <param name="canDownloadMetadataFromWeb">Optional. Whether metadata should be downloaded from the web.</param>
    /// <param name="shouldSaveMetadataInMediaDirectories">Optional. Whether metadata should be saved in the media directories.</param>
    /// <param name="shouldSkipUnchangedDirectoriesDuringScan">Optional. Whether unchanged directories should be skipped during scan.</param>
    /// <returns>The created <see cref="AddLibraryRequest"/>.</returns>
    public AddLibraryRequest Create(
        string? title = null,
        string? libraryType = null,
        string[]? contentLocations = null,
        string? coverImage = null,
        bool? isEnabled = null,
        bool? isLocked = null,
        bool? canDownloadMetadataFromWeb = null,
        bool? shouldSaveMetadataInMediaDirectories = null,
        bool? shouldSkipUnchangedDirectoriesDuringScan = null)
    {
        return new AddLibraryRequest(
            title ?? _faker.Commerce.Department(),
            libraryType ?? "EBook",
            contentLocations ?? [_faker.System.DirectoryPath()],
            coverImage ?? null,
            isEnabled ?? _faker.Random.Bool(),
            isLocked ?? _faker.Random.Bool(),
            canDownloadMetadataFromWeb ?? _faker.Random.Bool(),
            shouldSaveMetadataInMediaDirectories ?? _faker.Random.Bool(),
            shouldSkipUnchangedDirectoriesDuringScan ?? _faker.Random.Bool()
        );
    }

    /// <summary>
    /// Creates a list of <see cref="AddLibraryRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<AddLibraryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
