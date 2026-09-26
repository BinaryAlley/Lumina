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
using Lumina.Contracts.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="TrackResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackResponseFixture
{
    private readonly Faker _faker = new();
    private readonly AudioMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackResponse"/>.
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
    /// <param name="work">Optional. The title of the work the track is a recording of.</param>
    /// <param name="musicBrainzRecordingId">Optional. The MusicBrainz recording Id of the track.</param>
    /// <param name="musicBrainzTrackId">Optional. The MusicBrainz track Id of the track.</param>
    /// <param name="musicBrainzWorkId">Optional. The MusicBrainz work Id of the track.</param>
    /// <param name="createdOnUtc">Optional. The date and time when the track was created.</param>
    /// <param name="updatedOnUtc">Optional. The date and time when the track was updated.</param>
    /// <param name="moods">Optional. The list of moods of the track.</param>
    /// <param name="isrcs">Optional. The list of ISRCs of the track.</param>
    /// <param name="contributors">Optional. The list of contributor references of the track.</param>
    /// <param name="ratings">Optional. The list of ratings of the track.</param>
    /// <param name="includeMusicBrainzRecordingId">Whether the MusicBrainz recording Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzTrackId">Whether the MusicBrainz track Id should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMusicBrainzWorkId">Whether the MusicBrainz work Id should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="TrackResponse"/>.</returns>
    public TrackResponse Create(
        Guid? id = null,
        Guid? albumId = null,
        Guid? libraryId = null,
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
        DateTime? createdOnUtc = null,
        DateTime? updatedOnUtc = null,
        List<MoodDto>? moods = null,
        List<IsrcDto>? isrcs = null,
        List<MediaContributorReferenceDto>? contributors = null,
        List<AudioRatingDto>? ratings = null,
        bool includeMusicBrainzRecordingId = true,
        bool includeMusicBrainzTrackId = true,
        bool includeMusicBrainzWorkId = true)
    {
        return new TrackResponse(
            id ?? Guid.NewGuid(),
            albumId ?? Guid.NewGuid(),
            libraryId ?? Guid.NewGuid(),
            path ?? _faker.System.FilePath(),
            metadata ?? _audioMetadataDtoFixture.Create(),
            trackNumber ?? _faker.Random.Int(1, 30),
            discNumber ?? (_faker.Random.Bool() ? _faker.Random.Int(1, 3) : null),
            script ?? (_faker.Random.Bool() ? _faker.Random.AlphaNumeric(4) : null),
            key ?? (_faker.Random.Bool() ? _faker.PickRandom<MusicKey>() : null),
            bpm ?? (_faker.Random.Bool() ? _faker.Random.Int(40, 220) : null),
            work ?? (_faker.Random.Bool() ? _faker.Lorem.Sentence() : null),
            includeMusicBrainzRecordingId ? (musicBrainzRecordingId ?? Guid.NewGuid()) : null,
            includeMusicBrainzTrackId ? (musicBrainzTrackId ?? Guid.NewGuid()) : null,
            includeMusicBrainzWorkId ? (musicBrainzWorkId ?? Guid.NewGuid()) : null,
            createdOnUtc ?? _faker.Date.Past().ToUniversalTime(),
            updatedOnUtc ?? (_faker.Random.Bool() ? _faker.Date.Recent().ToUniversalTime() : null),
            moods ?? _moodDtoFixture.CreateMany(1),
            isrcs ?? _isrcDtoFixture.CreateMany(1),
            contributors ?? _mediaContributorReferenceDtoFixture.CreateMany(1),
            ratings ?? _audioRatingDtoFixture.CreateMany(1));
    }

    /// <summary>
    /// Creates a list of <see cref="TrackResponse"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<TrackResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
