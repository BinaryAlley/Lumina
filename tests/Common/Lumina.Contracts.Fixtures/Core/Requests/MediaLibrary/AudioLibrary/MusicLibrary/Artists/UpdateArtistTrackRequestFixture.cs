#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.Common;
using Lumina.Contracts.DTO.MediaContributors;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.Common;
using Lumina.Contracts.Fixtures.Core.DTO.MediaContributors;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary;
using Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Artists;

/// <summary>
/// Fixture class for the <see cref="UpdateArtistTrackRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateArtistTrackRequestFixture
{
    private readonly Faker _faker = new();
    private readonly AudioMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a random valid request to update a track of an album of an artist.
    /// </summary>
    /// <param name="trackId">Optional. The Id of the track, when the track already exists.</param>
    /// <param name="path">Optional. The file system path of the track.</param>
    /// <param name="metadata">Optional. The audio metadata of the track.</param>
    /// <param name="trackNumber">Optional. The number of the track on its disc.</param>
    /// <param name="discNumber">Optional. The number of the disc the track belongs to.</param>
    /// <param name="script">Optional. The script used by the language of the track.</param>
    /// <param name="key">Optional. The musical key of the track.</param>
    /// <param name="bpm">Optional. The tempo of the track in beats per minute.</param>
    /// <param name="work">Optional. The title of the work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">Optional. The MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">Optional. The MusicBrainz identifier of the track.</param>
    /// <param name="musicBrainzWorkId">Optional. The MusicBrainz identifier of the work.</param>
    /// <param name="moods">Optional. The moods of the track.</param>
    /// <param name="isrcs">Optional. The ISRC codes of the track.</param>
    /// <param name="contributors">Optional. The media contributors that performed on the track.</param>
    /// <param name="ratings">Optional. The ratings of the track.</param>
    /// <param name="includeTrackId">Whether the track Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePath">Whether the path should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTrackNumber">Whether the track number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeDiscNumber">Whether the disc number should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeScript">Whether the script should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeKey">Whether the musical key should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBpm">Whether the tempo should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeWork">Whether the work should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzRecordingId">Whether the MusicBrainz recording Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzTrackId">Whether the MusicBrainz track Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzWorkId">Whether the MusicBrainz work Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMoods">Whether the moods should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsrcs">Whether the ISRC codes should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors list should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings list should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created request to update a track of an album of an artist.</returns>
    public UpdateArtistTrackRequest Create(
        Guid? trackId = null,
        string? path = null,
        AudioMetadataDto? metadata = null,
        int? trackNumber = null,
        int? discNumber = null,
        string? script = null,
        MusicKey? key = null,
        int? bpm = null,
        string? work = null,
        Guid? musicBrainzRecordingId = null,
        Guid? musicBrainzTrackId = null,
        Guid? musicBrainzWorkId = null,
        List<MoodDto>? moods = null,
        List<IsrcDto>? isrcs = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AudioRatingDto>? ratings = null,
        bool includeTrackId = true,
        bool includePath = true,
        bool includeMetadata = true,
        bool includeTrackNumber = true,
        bool includeDiscNumber = true,
        bool includeScript = true,
        bool includeKey = true,
        bool includeBpm = true,
        bool includeWork = true,
        bool includeMusicBrainzRecordingId = true,
        bool includeMusicBrainzTrackId = true,
        bool includeMusicBrainzWorkId = true,
        bool includeMoods = true,
        bool includeIsrcs = true,
        bool includeContributors = true,
        bool includeRatings = true)
    {
        return new UpdateArtistTrackRequest(
            includeTrackId ? (trackId ?? _faker.Random.Guid()) : null,
            includePath ? (path ?? Path.Combine(Path.GetTempPath(), "lumina-tracks", _faker.System.FileName())) : null,
            includeMetadata ? (metadata ?? _audioMetadataDtoFixture.Create()) : null,
            includeTrackNumber ? (trackNumber ?? _faker.Random.Int(1, 30)) : null,
            includeDiscNumber ? (discNumber ?? _faker.Random.Int(1, 3)) : null,
            includeScript ? (script ?? _faker.Random.AlphaNumeric(4)) : null,
            includeKey ? (key ?? _faker.PickRandom<MusicKey>()) : null,
            includeBpm ? (bpm ?? _faker.Random.Int(40, 220)) : null,
            includeWork ? (work ?? _faker.Lorem.Sentence()) : null,
            includeMusicBrainzRecordingId ? (musicBrainzRecordingId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzTrackId ? (musicBrainzTrackId ?? _faker.Random.Guid()) : null,
            includeMusicBrainzWorkId ? (musicBrainzWorkId ?? _faker.Random.Guid()) : null,
            includeMoods ? (moods ?? [.. _moodDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeIsrcs ? (isrcs ?? [.. _isrcDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeContributors ? (contributors ?? [.. _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeRatings ? (ratings ?? [.. _audioRatingDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateArtistTrackRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateArtistTrackRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
