#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DataAccess.Entities.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="MusicLibraryScanItemMetadataEntity"/> class.
/// </summary>
[ExcludeFromCodeCoverage]
public class MusicLibraryScanItemMetadataEntityFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="MusicLibraryScanItemMetadataEntity"/>.
    /// </summary>
    /// <param name="id">Optional. The Id of the staged metadata.</param>
    /// <param name="libraryScanId">Optional. The Id of the media library scan the staged metadata belongs to.</param>
    /// <param name="libraryId">Optional. The Id of the media library the staged metadata belongs to.</param>
    /// <param name="path">Optional. The file system path of the music file.</param>
    /// <param name="artistName">Optional. The name of the artist.</param>
    /// <param name="releaseName">Optional. The name of the release.</param>
    /// <param name="trackTitle">Optional. The title of the track.</param>
    /// <param name="trackNumber">Optional. The number of the track on its disc.</param>
    /// <param name="discNumber">Optional. The number of the disc the track belongs to.</param>
    /// <param name="musicBrainzArtistId">Optional. The MusicBrainz identifier of the artist.</param>
    /// <param name="musicBrainzReleaseArtistId">Optional. The MusicBrainz identifier of the release artist.</param>
    /// <param name="musicBrainzReleaseGroupId">Optional. The MusicBrainz identifier of the release group.</param>
    /// <param name="musicBrainzReleaseId">Optional. The MusicBrainz identifier of the release.</param>
    /// <param name="musicBrainzRecordingId">Optional. The MusicBrainz identifier of the recording.</param>
    /// <param name="musicBrainzTrackId">Optional. The MusicBrainz identifier of the track.</param>
    /// <param name="musicBrainzWorkId">Optional. The MusicBrainz identifier of the work.</param>
    /// <param name="workTitle">Optional. The title of the work.</param>
    /// <param name="includeMusicBrainzTags">Whether the MusicBrainz identifiers and the work title should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="MusicLibraryScanItemMetadataEntity"/>.</returns>
    public MusicLibraryScanItemMetadataEntity Create(
        Guid? id = null,
        Guid? libraryScanId = null,
        Guid? libraryId = null,
        string? path = null,
        string? artistName = null,
        string? releaseName = null,
        string? trackTitle = null,
        int? trackNumber = null,
        int? discNumber = null,
        Guid? musicBrainzArtistId = null,
        Guid? musicBrainzReleaseArtistId = null,
        Guid? musicBrainzReleaseGroupId = null,
        Guid? musicBrainzReleaseId = null,
        Guid? musicBrainzRecordingId = null,
        Guid? musicBrainzTrackId = null,
        Guid? musicBrainzWorkId = null,
        string? workTitle = null,
        bool includeMusicBrainzTags = true)
    {
        return new MusicLibraryScanItemMetadataEntity
        {
            Id = id ?? Guid.NewGuid(),
            LibraryScanId = libraryScanId ?? Guid.NewGuid(),
            LibraryId = libraryId ?? Guid.NewGuid(),
            Path = path ?? _faker.System.FilePath(),
            ArtistName = artistName ?? _faker.Name.FullName(),
            ReleaseType = _faker.PickRandom<MusicReleaseType>(),
            ReleaseYear = Random.Shared.Next(1900, 2026),
            ReleaseName = releaseName ?? _faker.Music.Genre(),
            TrackTitle = trackTitle ?? _faker.Music.Genre(),
            TrackNumber = trackNumber ?? _faker.Random.Int(1, 20),
            DiscNumber = discNumber ?? _faker.Random.Int(1, 3),
            DurationInSeconds = Random.Shared.Next(60, 7200),
            SampleRate = _faker.PickRandom(44100, 48000, 96000),
            Channels = _faker.PickRandom(1, 2, 6),
            BitDepth = Random.Shared.Next(8, 32),
            AudioCodec = _faker.PickRandom("FLAC", "MP3", "AAC", "ALAC"),
            Bitrate = Random.Shared.Next(96, 1411),
            MusicBrainzArtistId = includeMusicBrainzTags ? musicBrainzArtistId ?? Guid.NewGuid() : null,
            MusicBrainzReleaseArtistId = includeMusicBrainzTags ? musicBrainzReleaseArtistId ?? Guid.NewGuid() : null,
            MusicBrainzReleaseGroupId = includeMusicBrainzTags ? musicBrainzReleaseGroupId ?? Guid.NewGuid() : null,
            MusicBrainzReleaseId = includeMusicBrainzTags ? musicBrainzReleaseId ?? Guid.NewGuid() : null,
            MusicBrainzRecordingId = includeMusicBrainzTags ? musicBrainzRecordingId ?? Guid.NewGuid() : null,
            MusicBrainzTrackId = includeMusicBrainzTags ? musicBrainzTrackId ?? Guid.NewGuid() : null,
            MusicBrainzWorkId = includeMusicBrainzTags ? musicBrainzWorkId ?? Guid.NewGuid() : null,
            WorkTitle = includeMusicBrainzTags ? workTitle ?? _faker.Music.Genre() : null
        };
    }

    /// <summary>
    /// Creates a list of <see cref="MusicLibraryScanItemMetadataEntity"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<MusicLibraryScanItemMetadataEntity> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
