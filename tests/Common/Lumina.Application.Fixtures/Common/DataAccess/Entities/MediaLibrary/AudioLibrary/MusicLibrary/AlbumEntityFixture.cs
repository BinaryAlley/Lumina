#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.Setup;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="AlbumEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class AlbumEntityFixture
{
    private readonly Random _random = new();
    private readonly Faker _faker = new();
    private readonly TrackEntityFixture _trackEntityFixture = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly AlbumContributorEntityFixture _albumContributorEntityFixture = new();
    private readonly AudioRatingEntityFixture _audioRatingEntityFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="AlbumEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the album.</param>
    /// <param name="artistId">Optional. The Id of the artist that owns the album.</param>
    /// <param name="libraryId">Optional. The Id of the media library that owns the album.</param>
    /// <param name="title">Optional. The title of the album.</param>
    /// <param name="tracks">Optional. The tracks of the album.</param>
    /// <param name="includeTracks">Whether the album should own a generated track, or no track at all.</param>
    /// <param name="includeMetadata">Whether the owned metadata collections (Tags, Genres, Contributors, Ratings) should be included, or forced to empty collections.</param>
    /// <param name="originalReleaseDate">Optional. The original release date of the album.</param>
    /// <param name="originalReleaseYear">Optional. The original release year of the album.</param>
    /// <param name="reReleaseDate">Optional. The re-release date of the album.</param>
    /// <param name="reReleaseYear">Optional. The re-release year of the album.</param>
    /// <param name="barcode">Optional. The barcode of the album.</param>
    /// <param name="includeOriginalReleaseDate">Whether the original release date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalReleaseYear">Whether the original release year should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReReleaseDate">Whether the re-release date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReReleaseYear">Whether the re-release year should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBarcode">Whether the barcode should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AlbumEntity"/>.</returns>
    public AlbumEntity Create(
        Guid? id = null,
        Guid? artistId = null,
        Guid? libraryId = null,
        string? title = null,
        List<TrackEntity>? tracks = null,
        bool includeTracks = true,
        bool includeMetadata = true,
        DateOnly? originalReleaseDate = null,
        int? originalReleaseYear = null,
        DateOnly? reReleaseDate = null,
        int? reReleaseYear = null,
        string? barcode = null,
        bool includeOriginalReleaseDate = true,
        bool includeOriginalReleaseYear = true,
        bool includeReReleaseDate = true,
        bool includeReReleaseYear = true,
        bool includeBarcode = true)
    {
        Guid resolvedId = id ?? Guid.NewGuid();
        Guid resolvedArtistId = artistId ?? Guid.NewGuid();
        Guid resolvedLibraryId = libraryId ?? Guid.NewGuid();
        const int MAXIMUM_RELEASE_YEAR = 2026;
        int resolvedOriginalReleaseYear = originalReleaseYear ?? _random.Next(1900, MAXIMUM_RELEASE_YEAR);
        int resolvedReReleaseYearUpperBound = Math.Max(resolvedOriginalReleaseYear + 1, MAXIMUM_RELEASE_YEAR);
        int resolvedReReleaseYear = reReleaseYear ?? _random.Next(resolvedOriginalReleaseYear, resolvedReReleaseYearUpperBound);
        DateOnly resolvedOriginalReleaseDate = originalReleaseDate ?? _faker.DateOnlyBetween(new DateOnly(resolvedOriginalReleaseYear, 1, 1), new DateOnly(resolvedOriginalReleaseYear, 12, 31));
        DateOnly resolvedReReleaseDate = reReleaseDate ?? _faker.DateOnlyBetween(resolvedReReleaseYear == resolvedOriginalReleaseYear ? resolvedOriginalReleaseDate : new DateOnly(resolvedReReleaseYear, 1, 1), new DateOnly(resolvedReReleaseYear, 12, 31));
        List<TrackEntity> resolvedTracks = tracks ?? (includeTracks ? [_trackEntityFixture.Create(albumId: resolvedId, libraryId: resolvedLibraryId, includeMetadata: includeMetadata, includeOriginalReleaseDate: includeOriginalReleaseDate, includeOriginalReleaseYear: includeOriginalReleaseYear, includeReReleaseDate: includeReReleaseDate, includeReReleaseYear: includeReReleaseYear)] : []);

        return new Faker<AlbumEntity>()
            .CustomInstantiator(f => new AlbumEntity
            {
                Id = resolvedId,
                ArtistId = resolvedArtistId,
                LibraryId = resolvedLibraryId,
                Title = default!,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = null
            })
            .RuleFor(x => x.Title, f => title ?? f.Random.String2(f.Random.Number(1, 255)))
            .RuleFor(x => x.OriginalTitle, f => f.Random.String2(f.Random.Number(1, 255)))
            .RuleFor(x => x.Description, f => f.Random.String2(f.Random.Number(1, 2000)))
            .RuleFor(x => x.OriginalReleaseDate, includeOriginalReleaseDate ? resolvedOriginalReleaseDate : (DateOnly?)null)
            .RuleFor(x => x.OriginalReleaseYear, includeOriginalReleaseYear ? resolvedOriginalReleaseYear : (int?)null)
            .RuleFor(x => x.ReReleaseDate, includeReReleaseDate ? resolvedReReleaseDate : (DateOnly?)null)
            .RuleFor(x => x.ReReleaseYear, includeReReleaseYear ? resolvedReReleaseYear : (int?)null)
            .RuleFor(x => x.ReleaseCountry, f => f.PickRandom<ReleaseCountry>())
            .RuleFor(x => x.ReleaseVersion, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.LanguageCode, f => f.Random.String2(2))
            .RuleFor(x => x.LanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.LanguageNativeName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.OriginalLanguageCode, f => f.Random.String2(2))
            .RuleFor(x => x.OriginalLanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.OriginalLanguageNativeName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.ReleaseType, f => f.PickRandom<MusicReleaseType>())
            .RuleFor(x => x.ReleaseStatus, f => f.PickRandom<MusicReleaseStatus>())
            .RuleFor(x => x.TotalDiscs, _random.Next(1, 3))
            .RuleFor(x => x.TotalTracks, resolvedTracks.Count)
            .RuleFor(x => x.MediaFormat, f => f.PickRandom<MusicMediaFormat>())
            .RuleFor(x => x.Barcode, f => includeBarcode ? (barcode ?? f.Random.String2(f.Random.Number(12, 13), "0123456789")) : null)
            .RuleFor(x => x.CatalogNumber, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.MusicBrainzReleaseId, f => f.Random.Guid())
            .RuleFor(x => x.MusicBrainzReleaseGroupId, f => f.Random.Guid())
            .RuleFor(x => x.MusicBrainzReleaseArtistId, f => f.Random.Guid())
            .RuleFor(x => x.Tags, f => includeMetadata ? [.. _tagEntityFixture.CreateMany(f.Random.Number(1, 3))] : [])
            .RuleFor(x => x.Genres, f => includeMetadata ? [.. _genreEntityFixture.CreateMany(f.Random.Number(1, 3))] : [])
            .RuleFor(x => x.Contributors, f => includeMetadata ? _albumContributorEntityFixture.CreateMany(f.Random.Number(1, 3), albumId: resolvedId) : [])
            .RuleFor(x => x.Ratings, f => includeMetadata ? _audioRatingEntityFixture.CreateMany(f.Random.Number(1, 3)) : [])
            .RuleFor(x => x.Tracks, resolvedTracks)
            .RuleFor(x => x.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(x => x.UpdatedOnUtc, f => f.Date.Recent())
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="AlbumEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="AlbumEntity"/> instances.</returns>
    public List<AlbumEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
