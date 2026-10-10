#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Domain.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;
using Lumina.Domain.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.Fixtures.Core.BoundedContexts.LibraryManagementBoundedContext.LibraryScanAggregate.ValueObjects;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Domain.Fixtures.Core.BoundedContexts.AudioLibraryBoundedContext.MusicLibraryAggregate.ValueObjects;

/// <summary>
/// Fixture class for the <see cref="MusicLibraryPathMetadata"/> domain value object.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicLibraryPathMetadataFixture
{
    private readonly Faker _faker = new();
    private readonly ParsedLibraryPathFixture _parsedLibraryPathFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicLibraryPathMetadata"/>.
    /// </summary>
    /// <param name="artistName">Optional. The name of the artist derived from the path.</param>
    /// <param name="releaseType">Optional. The type of the release derived from the path.</param>
    /// <param name="releaseYear">Optional. The release year derived from the path.</param>
    /// <param name="releaseName">Optional. The name of the release derived from the path.</param>
    /// <param name="trackNumber">Optional. The number of the track derived from the path.</param>
    /// <param name="discNumber">Optional. The number of the disc the track belongs to, derived from the path.</param>
    /// <param name="trackTitle">Optional. The title of the track derived from the path.</param>
    /// <param name="includeArtistName">Whether the artist name should be captured. When <see langword="false"/>, the artist name is not captured.</param>
    /// <param name="includeReleaseType">Whether the release type should be captured. When <see langword="false"/>, the release type is not captured.</param>
    /// <param name="includeReleaseYear">Whether the release year should be captured. When <see langword="false"/>, the release year is not captured.</param>
    /// <param name="includeReleaseName">Whether the release name should be captured. When <see langword="false"/>, the release name is not captured.</param>
    /// <param name="includeTrackNumber">Whether the track number should be captured. When <see langword="false"/>, the track number is not captured.</param>
    /// <param name="includeDiscNumber">Whether the disc number should be captured. When <see langword="false"/>, the disc number is not captured.</param>
    /// <param name="includeTrackTitle">Whether the track title should be captured. When <see langword="false"/>, the track title is not captured.</param>
    /// <returns>The created <see cref="MusicLibraryPathMetadata"/>.</returns>
    public MusicLibraryPathMetadata Create(
        string? artistName = null,
        MusicReleaseType? releaseType = null,
        int? releaseYear = null,
        string? releaseName = null,
        int? trackNumber = null,
        int? discNumber = null,
        string? trackTitle = null,
        bool includeArtistName = true,
        bool includeReleaseType = true,
        bool includeReleaseYear = true,
        bool includeReleaseName = true,
        bool includeTrackNumber = true,
        bool includeDiscNumber = true,
        bool includeTrackTitle = true)
    {
        Dictionary<LibraryPathPartKind, string> values = [];
        if (includeArtistName)
            values[LibraryPathPartKind.Artist] = artistName ?? _faker.Name.FullName();
        if (includeReleaseType)
            values[LibraryPathPartKind.ReleaseType] = (releaseType ?? _faker.PickRandom<MusicReleaseType>()).ToString();
        if (includeReleaseYear)
            values[LibraryPathPartKind.ReleaseYear] = (releaseYear ?? _faker.Random.Int(1900, 2025)).ToString();
        if (includeReleaseName)
            values[LibraryPathPartKind.ReleaseName] = releaseName ?? _faker.Commerce.ProductName();
        if (includeTrackNumber)
            values[LibraryPathPartKind.TrackNumber] = (trackNumber ?? _faker.Random.Int(1, 30)).ToString();
        if (includeDiscNumber)
            values[LibraryPathPartKind.DiscNumber] = (discNumber ?? _faker.Random.Int(1, 4)).ToString();
        if (includeTrackTitle)
            values[LibraryPathPartKind.TrackName] = trackTitle ?? _faker.Commerce.ProductName();
        return MusicLibraryPathMetadata.FromParsedLibraryPath(_parsedLibraryPathFixture.Create(values));
    }

    /// <summary>
    /// Creates multiple <see cref="MusicLibraryPathMetadata"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicLibraryPathMetadata"/> instances.</returns>
    public List<MusicLibraryPathMetadata> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
