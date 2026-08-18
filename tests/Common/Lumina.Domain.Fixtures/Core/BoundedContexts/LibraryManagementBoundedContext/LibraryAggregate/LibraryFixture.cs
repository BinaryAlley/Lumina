#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Common.Primitives;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.UserManagementBoundedContext.UserAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;

/// <summary>
/// Fixture class for the <see cref="Library"/> domain aggregate.
/// </summary>
[ExcludeFromCodeCoverage]
public class LibraryFixture
{
    private readonly Faker _faker = new();
    private readonly Random _random = new();

    /// <summary>
    /// Creates a random valid <see cref="Library"/> domain aggregate.
    /// </summary>
    /// <param name="id">Optional. The library Id.</param>
    /// <param name="userId">Optional. The Id of the user that owns the library.</param>
    /// <param name="title">Optional. The library title.</param>
    /// <param name="libraryType">Optional. The library type.</param>
    /// <param name="contentLocations">Optional. The content locations of the library.</param>
    /// <param name="coverImage">Optional. The cover image of the library.</param>
    /// <param name="includeCoverImage">Whether to include a cover image or not.</param>
    /// <param name="isEnabled">Whether the library is enabled.</param>
    /// <param name="isLocked">Whether the library is locked.</param>
    /// <param name="downloadMetadataFromWeb">Whether metadata download from the web is enabled.</param>
    /// <param name="shouldSaveMetadataInMediaDirectories">Whether metadata should be saved in the media directories.</param>
    /// <param name="shouldSkipUnchangedDirectoriesDuringScan">Whether unchanged directories should be skipped during scan.</param>
    /// <param name="scanIds">Optional. The scan Ids associated with the library.</param>
    /// <returns>The created <see cref="Library"/>.</returns>
    public Library Create(
        Guid? id = null,
        Guid? userId = null,
        string? title = null,
        LibraryType? libraryType = null,
        IEnumerable<string>? contentLocations = null,
        string? coverImage = null,
        bool includeCoverImage = true,
        bool isEnabled = true,
        bool isLocked = false,
        bool downloadMetadataFromWeb = true,
        bool shouldSaveMetadataInMediaDirectories = false,
        bool shouldSkipUnchangedDirectoriesDuringScan = false,
        IEnumerable<Guid>? scanIds = null)
    {
        List<string> validPaths =
        [
            "C:/Media",
            "D:/Books",
            "E:/Digital Library",
            "F:/Content"
        ];

        List<ScanId> resolvedScanIds = scanIds is null ? [ScanId.CreateUnique(), ScanId.CreateUnique()] : [.. scanIds.Select(scanId => ScanId.Create(scanId))];
        string? resolvedCoverImage = includeCoverImage ? (coverImage ?? _faker.System.FilePath()) : null;

        Result<Library> library = id is null ?
            Library.Create(
                userId is not null ? UserId.Create(userId.Value) : UserId.CreateUnique(),
                title ?? _faker.Random.String2(_faker.Random.Number(1, 50)),
                libraryType ?? _faker.PickRandom<LibraryType>(),
                contentLocations ?? validPaths.Take(_random.Next(1, validPaths.Count)),
                resolvedCoverImage,
                isEnabled,
                isLocked,
                downloadMetadataFromWeb,
                shouldSaveMetadataInMediaDirectories,
                shouldSkipUnchangedDirectoriesDuringScan,
                resolvedScanIds
            ) :
            Library.Create(
                LibraryId.Create(id.Value),
                userId is not null ? UserId.Create(userId.Value) : UserId.CreateUnique(),
                title ?? _faker.Random.String2(_faker.Random.Number(1, 50)),
                libraryType ?? _faker.PickRandom<LibraryType>(),
                contentLocations ?? validPaths.Take(_random.Next(1, validPaths.Count)),
                resolvedCoverImage,
                isEnabled,
                isLocked,
                downloadMetadataFromWeb,
                shouldSaveMetadataInMediaDirectories,
                shouldSkipUnchangedDirectoriesDuringScan,
                resolvedScanIds
            );

        return library.Value;
    }

    /// <summary>
    /// Creates multiple <see cref="Library"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="Library"/> instances.</returns>
    public List<Library> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
