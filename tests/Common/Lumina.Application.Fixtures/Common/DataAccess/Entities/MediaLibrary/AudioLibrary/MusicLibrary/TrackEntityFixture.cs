#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.Common;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Application.Fixtures.Common.DataAccess.Entities.Common;
using Lumina.Application.Fixtures.Common.Setup;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.Common;
using Lumina.Domain.SharedKernel.Common.Enums.MediaLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="TrackEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackEntityFixture
{
    private readonly Faker _faker = new();
    private readonly TagEntityFixture _tagEntityFixture = new();
    private readonly GenreEntityFixture _genreEntityFixture = new();
    private readonly TrackMoodEntityFixture _trackMoodEntityFixture = new();
    private readonly TrackIsrcEntityFixture _trackIsrcEntityFixture = new();
    private readonly TrackContributorEntityFixture _trackContributorEntityFixture = new();
    private readonly AudioRatingEntityFixture _audioRatingEntityFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the track.</param>
    /// <param name="albumId">Optional. The Id of the album that owns the track.</param>
    /// <param name="libraryId">Optional. The Id of the media library that owns the track.</param>
    /// <param name="path">Optional. The file system path of the track.</param>
    /// <param name="title">Optional. The title of the track.</param>
    /// <param name="trackNumber">Optional. The number of the track on its disc.</param>
    /// <param name="originalReleaseDate">Optional. The original release date of the track.</param>
    /// <param name="originalReleaseYear">Optional. The original release year of the track.</param>
    /// <param name="reReleaseDate">Optional. The re-release date of the track.</param>
    /// <param name="reReleaseYear">Optional. The re-release year of the track.</param>
    /// <param name="includeMetadata">Whether the owned metadata collections (Tags, Genres, Moods, ISRCs, Contributors, Ratings) should be included, or forced to empty collections.</param>
    /// <param name="includeOriginalReleaseDate">Whether the original release date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeOriginalReleaseYear">Whether the original release year should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReReleaseDate">Whether the re-release date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeReReleaseYear">Whether the re-release year should be included, or forced to <see langword="null"/>.</param>
    /// <param name="metadataStatus">Optional. The metadata enrichment status of the track.</param>
    /// <returns>The created <see cref="TrackEntity"/>.</returns>
    public TrackEntity Create(
        Guid? id = null,
        Guid? albumId = null,
        Guid? libraryId = null,
        string? path = null,
        string? title = null,
        int? trackNumber = null,
        DateOnly? originalReleaseDate = null,
        int? originalReleaseYear = null,
        DateOnly? reReleaseDate = null,
        int? reReleaseYear = null,
        bool includeMetadata = true,
        bool includeOriginalReleaseDate = true,
        bool includeOriginalReleaseYear = true,
        bool includeReReleaseDate = true,
        bool includeReReleaseYear = true,
        MetadataStatus? metadataStatus = null)
    {
        Guid resolvedId = id ?? Guid.NewGuid();
        Guid resolvedAlbumId = albumId ?? Guid.NewGuid();
        Guid resolvedLibraryId = libraryId ?? Guid.NewGuid();
        const int MAXIMUM_RELEASE_YEAR = 2026;
        int resolvedOriginalReleaseYear = originalReleaseYear ?? Random.Shared.Next(1900, MAXIMUM_RELEASE_YEAR);
        int resolvedReReleaseYearUpperBound = Math.Max(resolvedOriginalReleaseYear + 1, MAXIMUM_RELEASE_YEAR);
        int resolvedReReleaseYear = reReleaseYear ?? Random.Shared.Next(resolvedOriginalReleaseYear, resolvedReReleaseYearUpperBound);
        DateOnly resolvedOriginalReleaseDate = originalReleaseDate ?? _faker.DateOnlyBetween(new DateOnly(resolvedOriginalReleaseYear, 1, 1), new DateOnly(resolvedOriginalReleaseYear, 12, 31));
        DateOnly resolvedReReleaseDate = reReleaseDate ?? _faker.DateOnlyBetween(resolvedReReleaseYear == resolvedOriginalReleaseYear ? resolvedOriginalReleaseDate : new DateOnly(resolvedReReleaseYear, 1, 1), new DateOnly(resolvedReReleaseYear, 12, 31));

        return new Faker<TrackEntity>()
            .CustomInstantiator(f => new TrackEntity
            {
                Id = resolvedId,
                AlbumId = resolvedAlbumId,
                LibraryId = resolvedLibraryId,
                MetadataStatus = metadataStatus ?? MetadataStatus.Pending,
                Path = default!,
                Title = default!,
                CreatedOnUtc = default,
                CreatedBy = default,
                UpdatedBy = null
            })
            .RuleFor(x => x.Path, f => path ?? f.System.FilePath())
            .RuleFor(x => x.Title, f => title ?? f.Random.String2(f.Random.Number(1, 255)))
            .RuleFor(x => x.OriginalTitle, f => f.Random.String2(f.Random.Number(1, 255)))
            .RuleFor(x => x.Description, f => f.Random.String2(f.Random.Number(1, 2000)))
            .RuleFor(x => x.OriginalReleaseDate, includeOriginalReleaseDate ? resolvedOriginalReleaseDate : null)
            .RuleFor(x => x.OriginalReleaseYear, includeOriginalReleaseYear ? resolvedOriginalReleaseYear : null)
            .RuleFor(x => x.ReReleaseDate, includeReReleaseDate ? resolvedReReleaseDate : null)
            .RuleFor(x => x.ReReleaseYear, includeReReleaseYear ? resolvedReReleaseYear : null)
            .RuleFor(x => x.ReleaseCountry, f => f.PickRandom<ReleaseCountry>())
            .RuleFor(x => x.ReleaseVersion, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.LanguageCode, f => f.Random.String2(2))
            .RuleFor(x => x.LanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.LanguageNativeName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.OriginalLanguageCode, f => f.Random.String2(2))
            .RuleFor(x => x.OriginalLanguageName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.OriginalLanguageNativeName, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.DurationInSeconds, Random.Shared.Next(60, 7200))
            .RuleFor(x => x.SampleRate, f => f.PickRandom(44100, 48000, 96000))
            .RuleFor(x => x.Channels, f => f.PickRandom(1, 2, 6))
            .RuleFor(x => x.BitDepth, Random.Shared.Next(8, 32))
            .RuleFor(x => x.AudioCodec, f => f.PickRandom("FLAC", "MP3", "AAC", "ALAC"))
            .RuleFor(x => x.Bitrate, Random.Shared.Next(96, 1411))
            .RuleFor(x => x.TrackNumber, f => trackNumber ?? f.Random.Int(1, 20))
            .RuleFor(x => x.DiscNumber, Random.Shared.Next(1, 3))
            .RuleFor(x => x.Script, f => f.Random.String2(f.Random.Number(1, 50)))
            .RuleFor(x => x.Key, f => f.PickRandom<MusicKey>())
            .RuleFor(x => x.Bpm, Random.Shared.Next(40, 240))
            .RuleFor(x => x.IsVideo, f => f.Random.Bool())
            .RuleFor(x => x.WorkTitle, f => f.Music.Genre())
            .RuleFor(x => x.WorkType, f => f.Music.Genre())
            .RuleFor(x => x.MusicBrainzRecordingId, f => f.Random.Guid())
            .RuleFor(x => x.MusicBrainzTrackId, f => f.Random.Guid())
            .RuleFor(x => x.MusicBrainzWorkId, f => f.Random.Guid())
            .RuleFor(x => x.Tags, f => includeMetadata ? [.. _tagEntityFixture.CreateMany(f.Random.Number(1, 3))] : [])
            .RuleFor(x => x.Genres, f => includeMetadata ? [.. _genreEntityFixture.CreateMany(f.Random.Number(1, 3))] : [])
            .RuleFor(x => x.Moods, f => includeMetadata ? _trackMoodEntityFixture.CreateMany(f.Random.Number(1, 3)) : [])
            .RuleFor(x => x.Isrcs, f => includeMetadata ? _trackIsrcEntityFixture.CreateMany(f.Random.Number(1, 3)) : [])
            .RuleFor(x => x.Contributors, f => includeMetadata ? _trackContributorEntityFixture.CreateMany(f.Random.Number(1, 3), trackId: resolvedId) : [])
            .RuleFor(x => x.Ratings, f => includeMetadata ? _audioRatingEntityFixture.CreateMany(f.Random.Number(1, 3)) : [])
            .RuleFor(x => x.CreatedOnUtc, f => f.Date.Past())
            .RuleFor(x => x.UpdatedOnUtc, f => f.Date.Recent())
            .Generate();
    }

    /// <summary>
    /// Creates a list of <see cref="TrackEntity"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TrackEntity"/> instances.</returns>
    public List<TrackEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
