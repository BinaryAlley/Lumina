#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary;

/// <summary>
/// Fixture class for the <see cref="TrackMetadataLookupDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackMetadataLookupDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackMetadataLookupDto"/>.
    /// </summary>
    /// <param name="libraryId">Optional. The Id of the media library the track belongs to.</param>
    /// <param name="path">Optional. The file system path of the track.</param>
    /// <param name="musicBrainzRecordingId">Optional. The MusicBrainz identifier of the recording of the track.</param>
    /// <param name="isrc">Optional. The ISRC of the track.</param>
    /// <param name="title">Optional. The title of the track.</param>
    /// <param name="artistName">Optional. The name of the artist of the track.</param>
    /// <param name="releaseName">Optional. The name of the release the track belongs to.</param>
    /// <param name="trackNumber">Optional. The number of the track on its disc.</param>
    /// <param name="durationInSeconds">Optional. The duration of the track in seconds.</param>
    /// <param name="musicBrainzReleaseId">Optional. The MusicBrainz identifier of the release the track belongs to.</param>
    /// <param name="discNumber">Optional. The number of the disc the track belongs to.</param>
    /// <param name="musicBrainzWorkId">Optional. The MusicBrainz identifier of the work the track is a recording of.</param>
    /// <param name="workTitle">Optional. The title of the work the track is a recording of.</param>
    /// <returns>The created <see cref="TrackMetadataLookupDto"/>.</returns>
    public TrackMetadataLookupDto Create(
        Guid? libraryId = null,
        string? path = null,
        Guid? musicBrainzRecordingId = null,
        string? isrc = null,
        string? title = null,
        string? artistName = null,
        string? releaseName = null,
        int? trackNumber = null,
        int? durationInSeconds = null,
        Guid? musicBrainzReleaseId = null,
        int? discNumber = null,
        Guid? musicBrainzWorkId = null,
        string? workTitle = null)
    {
        return new TrackMetadataLookupDto(
            libraryId ?? Guid.NewGuid(),
            path ?? _faker.System.FilePath(),
            musicBrainzRecordingId,
            isrc,
            title,
            artistName,
            releaseName,
            trackNumber,
            durationInSeconds,
            musicBrainzReleaseId,
            discNumber,
            musicBrainzWorkId,
            workTitle);
    }

    /// <summary>
    /// Creates a list of <see cref="TrackMetadataLookupDto"/>.
    /// </summary>
    /// <param name="count">The number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<TrackMetadataLookupDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
