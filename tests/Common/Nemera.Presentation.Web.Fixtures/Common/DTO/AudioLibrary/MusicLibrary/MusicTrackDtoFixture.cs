#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary;
using Lumina.Presentation.Web.Common.DTO.AudioLibrary.MusicLibrary;
using Lumina.Presentation.Web.Common.DTO.Common;
using Lumina.Presentation.Web.Common.DTO.MediaContributors;
using Lumina.Presentation.Web.Common.Enums.AudioLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary;
using Lumina.Presentation.Web.Fixtures.Common.DTO.Common;
using Lumina.Presentation.Web.Fixtures.Common.DTO.MediaContributors;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.DTO.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicTrackDto"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicTrackDtoFixture
{
    private readonly Faker _faker = new();
    private readonly MusicTrackMetadataDtoFixture _musicTrackMetadataDtoFixture = new();
    private readonly MusicWorkDtoFixture _musicWorkDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a new <see cref="MusicTrackDto"/> instance with randomized test data.
    /// </summary>
    /// <param name="id">Optional. The Id of the track.</param>
    /// <param name="albumId">Optional. The Id of the album the track belongs to.</param>
    /// <param name="libraryId">Optional. The Id of the media library this track belongs to.</param>
    /// <param name="path">Optional. The file system path of the track.</param>
    /// <param name="metadata">Optional. The audio metadata of the track.</param>
    /// <param name="trackNumber">Optional. The number of the track on its disc.</param>
    /// <param name="discNumber">Optional. The number of the disc the track belongs to.</param>
    /// <param name="script">Optional. The script used by the language of the track.</param>
    /// <param name="key">Optional. The musical key of the track.</param>
    /// <param name="bpm">Optional. The tempo of the track in beats per minute.</param>
    /// <param name="work">Optional. The work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">Optional. The MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">Optional. The MusicBrainz identifier of the track.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the track was created.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the track was updated.</param>
    /// <param name="moods">Optional. The moods of the track.</param>
    /// <param name="isrcs">Optional. The ISRC codes of the track.</param>
    /// <param name="contributors">Optional. The references to the media contributors that performed on the track.</param>
    /// <param name="ratings">Optional. The ratings of the track.</param>
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDiscNumber">Whether the disc number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeScript">Whether the script should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeKey">Whether the musical key should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBpm">Whether the tempo should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeWork">Whether the work should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzRecordingId">Whether the MusicBrainz recording Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzTrackId">Whether the MusicBrainz track Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeUpdatedOnUtc">Whether the update date should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMoods">Whether the moods should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsrcs">Whether the ISRC codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MusicTrackDto"/> instance.</returns>
    public MusicTrackDto Create(
        Guid? id = null,
        Guid? albumId = null,
        Guid? libraryId = null,
        string? path = null,
        MusicTrackMetadataDto? metadata = null,
        int? trackNumber = null,
        int? discNumber = null,
        string? script = null,
        MusicKey? key = null,
        int? bpm = null,
        MusicWorkDto? work = null,
        Guid? musicBrainzRecordingId = null,
        Guid? musicBrainzTrackId = null,
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null,
        List<MoodDto>? moods = null,
        List<IsrcDto>? isrcs = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AudioRatingDto>? ratings = null,
        bool includeMetadata = true,
        bool includeDiscNumber = true,
        bool includeScript = true,
        bool includeKey = true,
        bool includeBpm = true,
        bool includeWork = true,
        bool includeMusicBrainzRecordingId = true,
        bool includeMusicBrainzTrackId = true,
        bool includeUpdatedOnUtc = true,
        bool includeMoods = true,
        bool includeIsrcs = true,
        bool includeContributors = true,
        bool includeRatings = true)
    {
        return new MusicTrackDto
        {
            Id = id ?? _faker.Random.Guid(),
            AlbumId = albumId ?? _faker.Random.Guid(),
            LibraryId = libraryId ?? _faker.Random.Guid(),
            Path = path ?? _faker.System.FilePath(),
            Metadata = includeMetadata ? (metadata ?? _musicTrackMetadataDtoFixture.Create()) : null,
            TrackNumber = trackNumber ?? _faker.Random.Number(1, 30),
            DiscNumber = includeDiscNumber ? (discNumber ?? _faker.Random.Number(1, 3)) : null,
            Script = includeScript ? (script ?? _faker.Lorem.Word()) : null,
            Key = includeKey ? (key ?? _faker.PickRandom<MusicKey>()) : null,
            Bpm = includeBpm ? (bpm ?? _faker.Random.Number(60, 200)) : null,
            Work = includeWork ? (work ?? _musicWorkDtoFixture.Create()) : null,
            MusicBrainzRecordingId = includeMusicBrainzRecordingId ? (musicBrainzRecordingId ?? _faker.Random.Guid()) : null,
            MusicBrainzTrackId = includeMusicBrainzTrackId ? (musicBrainzTrackId ?? _faker.Random.Guid()) : null,
            CreatedOnUtc = createdOnUtc ?? _faker.Date.Past(),
            UpdatedOnUtc = includeUpdatedOnUtc ? (updatedOnUtc ?? _faker.Date.Recent()) : null,
            Moods = includeMoods ? (moods ?? [.. _moodDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Isrcs = includeIsrcs ? (isrcs ?? [.. _isrcDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Contributors = includeContributors ? (contributors ?? [.. _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null,
            Ratings = includeRatings ? (ratings ?? [.. _audioRatingDtoFixture.CreateMany(_faker.Random.Number(1, 3))]) : null
        };
    }

    /// <summary>
    /// Creates multiple <see cref="MusicTrackDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MusicTrackDto"/> instances.</returns>
    public List<MusicTrackDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
