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
using Lumina.Contracts.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="UpdateTrackRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdateTrackRequestFixture
{
    private readonly Faker _faker = new();
    private readonly AudioMetadataDtoFixture _audioMetadataDtoFixture = new();
    private readonly MoodDtoFixture _moodDtoFixture = new();
    private readonly IsrcDtoFixture _isrcDtoFixture = new();
    private readonly MediaContributorReferenceDtoFixture _mediaContributorReferenceDtoFixture = new();
    private readonly AudioRatingDtoFixture _audioRatingDtoFixture = new();

    /// <summary>
    /// Creates a random valid request to update a track.
    /// </summary>
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
    /// <param name="includeOptionalProperties">Whether the optional properties that are not explicitly provided should be randomized, or forced to <see langword="null"/>.</param>
    /// <param name="includeContributors">Whether the contributors list should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeRatings">Whether the ratings list should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includePath">Whether the path should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeMetadata">Whether the metadata should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeTrackNumber">Whether the track number should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created request to update a track.</returns>
    public UpdateTrackRequest Create(
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
        bool includeOptionalProperties = true,
        bool includeContributors = true,
        bool includeRatings = true,
        bool includePath = true,
        bool includeMetadata = true,
        bool includeTrackNumber = true)
    {
        return new UpdateTrackRequest(
            includePath ? (path ?? _faker.System.FilePath()) : null,
            includeMetadata ? (metadata ?? _audioMetadataDtoFixture.Create()) : null,
            includeTrackNumber ? (trackNumber ?? _faker.Random.Int(1, 20)) : null,
            includeOptionalProperties ? (discNumber ?? _faker.Random.Int(1, 3)) : null,
            includeOptionalProperties ? (script ?? _faker.Random.String2(_faker.Random.Number(1, 50))) : null,
            includeOptionalProperties ? (key ?? _faker.PickRandom<MusicKey>()) : null,
            includeOptionalProperties ? (bpm ?? _faker.Random.Int(40, 240)) : null,
            includeOptionalProperties ? (work ?? _faker.Music.Genre()) : null,
            includeOptionalProperties ? (musicBrainzRecordingId ?? _faker.Random.Guid()) : null,
            includeOptionalProperties ? (musicBrainzTrackId ?? _faker.Random.Guid()) : null,
            includeOptionalProperties ? (musicBrainzWorkId ?? _faker.Random.Guid()) : null,
            includeOptionalProperties ? (moods ?? [.. _moodDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeOptionalProperties ? (isrcs ?? [.. _isrcDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeContributors ? (contributors ?? [.. _mediaContributorReferenceDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null,
            includeRatings ? (ratings ?? [.. _audioRatingDtoFixture.CreateMany(_faker.Random.Int(1, 3))]) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdateTrackRequest"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdateTrackRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
