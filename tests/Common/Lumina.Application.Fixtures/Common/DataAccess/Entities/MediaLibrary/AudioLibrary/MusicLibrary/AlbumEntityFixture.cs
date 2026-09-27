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
    /// <param name="originalReleaseDate">Optional. The original release date of the album.</param>
    /// <param name="originalReleaseYear">Optional. The original release year of the album.</param>
    /// <param name="reReleaseDate">Optional. The re-release date of the album.</param>
    /// <param name="reReleaseYear">Optional. The re-release year of the album.</param>
    /// <param name="barcode">Optional. The barcode of the album.</param>
    /// <param name="originalTitle">Optional. The original title of the album.</param>
    /// <param name="description">Optional. A brief description or summary of the album.</param>
    /// <param name="languageCode">Optional. The language code of the album.</param>
    /// <param name="languageName">Optional. The language name of the album.</param>
    /// <param name="languageNativeName">Optional. The language native name of the album.</param>
    /// <param name="originalLanguageCode">Optional. The original language code of the album.</param>
    /// <param name="originalLanguageName">Optional. The original language name of the album.</param>
    /// <param name="originalLanguageNativeName">Optional. The original language native name of the album.</param>
    /// <param name="releaseType">Optional. The release type of the album.</param>
    /// <param name="releaseStatus">Optional. The release status of the album.</param>
    /// <param name="totalDiscs">Optional. The total number of discs of the album.</param>
    /// <param name="mediaFormat">Optional. The media format of the album.</param>
    /// <param name="catalogNumber">Optional. The catalog number of the album.</param>
    /// <param name="musicBrainzReleaseId">Optional. The MusicBrainz release identifier of the album.</param>
    /// <param name="musicBrainzReleaseGroupId">Optional. The MusicBrainz release group identifier of the album.</param>
    /// <param name="musicBrainzReleaseArtistId">Optional. The MusicBrainz release artist identifier of the album.</param>
    /// <param name="includeTracks">Whether the album should own a generated track, or no track at all.</param>
    /// <param name="includeMetadata">Whether the owned metadata collections (Tags, Genres, Contributors, Ratings) should be included, or forced to empty collections.</param>
    /// <param name="includeOriginalReleaseDate">Whether the original release date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalReleaseYear">Whether the original release year should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReReleaseDate">Whether the re-release date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReReleaseYear">Whether the re-release year should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBarcode">Whether the barcode should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalTitle">Whether the original title should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDescription">Whether the description should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeLanguage">Whether the language of the album should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalLanguage">Whether the original language of the album should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseType">Whether the release type should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReleaseStatus">Whether the release status should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTotalDiscs">Whether the total number of discs should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMediaFormat">Whether the media format should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCatalogNumber">Whether the catalog number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseId">Whether the MusicBrainz release identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseGroupId">Whether the MusicBrainz release group identifier should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzReleaseArtistId">Whether the MusicBrainz release artist identifier should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="AlbumEntity"/>.</returns>
    public AlbumEntity Create(
        Guid? id = null,
        Guid? artistId = null,
        Guid? libraryId = null,
        string? title = null,
        List<TrackEntity>? tracks = null,
        DateOnly? originalReleaseDate = null,
        int? originalReleaseYear = null,
        DateOnly? reReleaseDate = null,
        int? reReleaseYear = null,
        string? barcode = null,
        string? originalTitle = null,
        string? description = null,
        string? languageCode = null,
        string? languageName = null,
        string? languageNativeName = null,
        string? originalLanguageCode = null,
        string? originalLanguageName = null,
        string? originalLanguageNativeName = null,
        MusicReleaseType? releaseType = null,
        MusicReleaseStatus? releaseStatus = null,
        int? totalDiscs = null,
        MusicMediaFormat? mediaFormat = null,
        string? catalogNumber = null,
        Guid? musicBrainzReleaseId = null,
        Guid? musicBrainzReleaseGroupId = null,
        Guid? musicBrainzReleaseArtistId = null,
        bool includeTracks = true,
        bool includeMetadata = true,
        bool includeOriginalReleaseDate = true,
        bool includeOriginalReleaseYear = true,
        bool includeReReleaseDate = true,
        bool includeReReleaseYear = true,
        bool includeBarcode = true,
        bool includeOriginalTitle = true,
        bool includeDescription = true,
        bool includeLanguage = true,
        bool includeOriginalLanguage = true,
        bool includeReleaseType = true,
        bool includeReleaseStatus = true,
        bool includeTotalDiscs = true,
        bool includeMediaFormat = true,
        bool includeCatalogNumber = true,
        bool includeMusicBrainzReleaseId = true,
        bool includeMusicBrainzReleaseGroupId = true,
        bool includeMusicBrainzReleaseArtistId = true)
    {
        Guid resolvedId = id ?? Guid.NewGuid();
        Guid resolvedArtistId = artistId ?? Guid.NewGuid();
        Guid resolvedLibraryId = libraryId ?? Guid.NewGuid();
        const int MAXIMUM_RELEASE_YEAR = 2026;
        int resolvedOriginalReleaseYear = originalReleaseYear ?? Random.Shared.Next(1900, MAXIMUM_RELEASE_YEAR);
        int resolvedReReleaseYearUpperBound = Math.Max(resolvedOriginalReleaseYear + 1, MAXIMUM_RELEASE_YEAR);
        int resolvedReReleaseYear = reReleaseYear ?? Random.Shared.Next(resolvedOriginalReleaseYear, resolvedReReleaseYearUpperBound);
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
            .RuleFor(x => x.OriginalTitle, f => includeOriginalTitle ? (originalTitle ?? f.Random.String2(f.Random.Number(1, 255))) : null)
            .RuleFor(x => x.Description, f => includeDescription ? (description ?? f.Random.String2(f.Random.Number(1, 2000))) : null)
            .RuleFor(x => x.OriginalReleaseDate, includeOriginalReleaseDate ? resolvedOriginalReleaseDate : (DateOnly?)null)
            .RuleFor(x => x.OriginalReleaseYear, includeOriginalReleaseYear ? resolvedOriginalReleaseYear : (int?)null)
            .RuleFor(x => x.ReReleaseDate, includeReReleaseDate ? resolvedReReleaseDate : (DateOnly?)null)
            .RuleFor(x => x.ReReleaseYear, includeReReleaseYear ? resolvedReReleaseYear : (int?)null)
            .RuleFor(x => x.ReleaseCountry, f => f.PickRandom<ReleaseCountry>())
            .RuleFor(x => x.ReleaseVersion, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.LanguageCode, f => includeLanguage ? (languageCode ?? f.Random.String2(2)) : null)
            .RuleFor(x => x.LanguageName, f => includeLanguage ? (languageName ?? f.Random.String2(f.Random.Number(1, 50))) : null)
            .RuleFor(x => x.LanguageNativeName, f => includeLanguage ? (languageNativeName ?? f.Random.String2(f.Random.Number(1, 50))) : null)
            .RuleFor(x => x.OriginalLanguageCode, f => includeOriginalLanguage ? (originalLanguageCode ?? f.Random.String2(2)) : null)
            .RuleFor(x => x.OriginalLanguageName, f => includeOriginalLanguage ? (originalLanguageName ?? f.Random.String2(f.Random.Number(1, 50))) : null)
            .RuleFor(x => x.OriginalLanguageNativeName, f => includeOriginalLanguage ? (originalLanguageNativeName ?? f.Random.String2(f.Random.Number(1, 50))) : null)
            .RuleFor(x => x.ReleaseType, f => includeReleaseType ? (releaseType ?? f.PickRandom<MusicReleaseType>()) : (MusicReleaseType?)null)
            .RuleFor(x => x.ReleaseStatus, f => includeReleaseStatus ? (releaseStatus ?? f.PickRandom<MusicReleaseStatus>()) : (MusicReleaseStatus?)null)
            .RuleFor(x => x.TotalDiscs, includeTotalDiscs ? (totalDiscs ?? Random.Shared.Next(1, 3)) : (int?)null)
            .RuleFor(x => x.TotalTracks, resolvedTracks.Count)
            .RuleFor(x => x.MediaFormat, f => includeMediaFormat ? (mediaFormat ?? f.PickRandom<MusicMediaFormat>()) : (MusicMediaFormat?)null)
            .RuleFor(x => x.Barcode, f => includeBarcode ? (barcode ?? f.Random.String2(f.Random.Number(12, 13), "0123456789")) : null)
            .RuleFor(x => x.CatalogNumber, f => includeCatalogNumber ? (catalogNumber ?? f.Random.String2(f.Random.Number(1, 50))) : null)
            .RuleFor(x => x.MusicBrainzReleaseId, f => includeMusicBrainzReleaseId ? (musicBrainzReleaseId ?? f.Random.Guid()) : (Guid?)null)
            .RuleFor(x => x.MusicBrainzReleaseGroupId, f => includeMusicBrainzReleaseGroupId ? (musicBrainzReleaseGroupId ?? f.Random.Guid()) : (Guid?)null)
            .RuleFor(x => x.MusicBrainzReleaseArtistId, f => includeMusicBrainzReleaseArtistId ? (musicBrainzReleaseArtistId ?? f.Random.Guid()) : (Guid?)null)
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
