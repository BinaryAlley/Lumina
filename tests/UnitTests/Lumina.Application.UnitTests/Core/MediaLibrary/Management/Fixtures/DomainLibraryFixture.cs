#region ========================================================================= USING =====================================================================================
using Bogus;
using ErrorOr;
using Lumina.Domain.Common.Enums.MediaLibrary;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryAggregate.ValueObjects;
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

        ErrorOr<Library> library = Library.Create(
            Guid.NewGuid(),
            _faker.Random.String2(_faker.Random.Number(1, 50)),
            _faker.PickRandom<LibraryType>(),
            validPaths.Take(_random.Next(1, validPaths.Count)),
            _faker.System.FilePath(),
            _faker.Random.Bool(),
            _faker.Random.Bool(),
            _faker.Random.Bool(),
            _faker.Random.Bool()
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
        bool downloadMedatadaFromWeb = true,
        bool saveMetadataInMediaDirectories = false)
    {
        ErrorOr<Library> library = id is null ?
            Library.Create(
                userId ?? Guid.NewGuid(),
                title ?? _faker.Random.String2(_faker.Random.Number(1, 50)),
                libraryType ?? LibraryType.Book,
                contentLocations ?? ["C:/Media"],
                coverImage,
                isEnabled, 
                isLocked, 
                downloadMedatadaFromWeb, 
                saveMetadataInMediaDirectories
            ) :
            Library.Create(
                LibraryId.Create(id.Value),
                userId ?? Guid.NewGuid(),
                title ?? _faker.Random.String2(_faker.Random.Number(1, 50)),
                libraryType ?? LibraryType.Book,
                contentLocations ?? ["C:/Media"],
                coverImage,
                isEnabled,
                isLocked,
                downloadMedatadaFromWeb,
                saveMetadataInMediaDirectories
            );

        return library.Value;
    }
}
