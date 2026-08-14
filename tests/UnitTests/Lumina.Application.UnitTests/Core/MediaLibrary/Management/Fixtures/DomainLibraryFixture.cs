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

namespace Lumina.Application.UnitTests.Core.MediaLibrary.Management.Fixtures;

/// <summary>
/// Fixture class for the <see cref="Library"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public class DomainLibraryFixture
{
    private readonly Faker _faker;
    private readonly Random _random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="DomainLibraryFixture"/> class.
    /// </summary>
    public DomainLibraryFixture()
    {
        _faker = new Faker();
    }

    /// <summary>
    /// Creates a media library domain entity.
    /// </summary>
    /// <returns>The created media library entity.</returns>
    public Library CreateLibrary()
    {
        List<string> validPaths =
        [
            "C:/Media",
            "D:/Books",
            "E:/Digital Library",
            "F:/Content"
        ];

        Result<Library> library = Library.Create(
            UserId.CreateUnique(),
            _faker.Random.String2(_faker.Random.Number(1, 50)),
            _faker.PickRandom<LibraryType>(),
            validPaths.Take(_random.Next(1, validPaths.Count)),
            _faker.System.FilePath(),
            _faker.Random.Bool(),
            _faker.Random.Bool(),
            _faker.Random.Bool(),
            _faker.Random.Bool(),
            _faker.Random.Bool(),
            [ScanId.CreateUnique(), ScanId.CreateUnique()]
        );

        return library.Value;
    }

    public Library CreateLibraryWithSpecificValues(
        Guid? id = null,
        Guid? userId = null,
        string? title = null,
        LibraryType? libraryType = null,
        IEnumerable<string>? contentLocations = null,
        string? coverImage = null,
        bool isEnabled = true,
        bool isLocked = false,
        bool downloadMetadataFromWeb = true,
        bool shouldSaveMetadataInMediaDirectories = false,
        bool shouldSkipUnchangedDirectoriesDuringScan = false,
        IEnumerable<Guid>? scanIds = null)
    {
        Result<Library> library = id is null ?
            Library.Create(
                userId is not null ? UserId.Create(userId.Value) : UserId.CreateUnique(),
                title ?? _faker.Random.String2(_faker.Random.Number(1, 50)),
                libraryType ?? LibraryType.Book,
                contentLocations ?? ["C:/Media"],
                coverImage,
                isEnabled, 
                isLocked, 
                downloadMetadataFromWeb, 
                shouldSaveMetadataInMediaDirectories,
                shouldSkipUnchangedDirectoriesDuringScan,
                scanIds?.Select(scanId => ScanId.Create(scanId)).ToList() ?? [ScanId.CreateUnique(), ScanId.CreateUnique()]
            ) :
            Library.Create(
                LibraryId.Create(id.Value),
                userId is not null ? UserId.Create(userId.Value) : UserId.CreateUnique(),
                title ?? _faker.Random.String2(_faker.Random.Number(1, 50)),
                libraryType ?? LibraryType.Book,
                contentLocations ?? ["C:/Media"],
                coverImage,
                isEnabled,
                isLocked,
                downloadMetadataFromWeb,
                shouldSaveMetadataInMediaDirectories,
                shouldSkipUnchangedDirectoriesDuringScan,
                scanIds?.Select(scanId => ScanId.Create(scanId)).ToList() ?? [ScanId.CreateUnique(), ScanId.CreateUnique()]
            );

        return library.Value;
    }
}
