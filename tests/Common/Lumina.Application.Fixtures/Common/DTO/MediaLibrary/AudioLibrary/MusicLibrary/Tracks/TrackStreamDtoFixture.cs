#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="TrackStreamDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackStreamDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackStreamDto"/>.
    /// </summary>
    /// <param name="path">Optional. The file system path of the track.</param>
    /// <param name="libraryId">Optional. The Id of the media library the track belongs to.</param>
    /// <param name="albumId">Optional. The Id of the album the track belongs to.</param>
    /// <param name="artistId">Optional. The Id of the artist that owns the album the track belongs to.</param>
    /// <param name="container">Optional. The container of the audio file of the track.</param>
    /// <param name="audioCodec">Optional. The audio codec used by the audio file of the track.</param>
    /// <param name="durationInSeconds">Optional. The duration of the audio of the track, in seconds.</param>
    /// <param name="sampleRate">Optional. The sample rate of the audio of the track, in Hz.</param>
    /// <param name="channels">Optional. The number of audio channels of the track.</param>
    /// <param name="bitDepth">Optional. The bit depth of the audio of the track.</param>
    /// <param name="bitrate">Optional. The bitrate of the audio of the track, in kbps.</param>
    /// <param name="includeContainer">Whether the container should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeAudioCodec">Whether the audio codec should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBitDepth">Whether the bit depth should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeBitrate">Whether the bitrate should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="TrackStreamDto"/>.</returns>
    public TrackStreamDto Create(
        string? path = null,
        Guid? libraryId = null,
        Guid? albumId = null,
        Guid? artistId = null,
        string? container = null,
        string? audioCodec = null,
        int? durationInSeconds = null,
        int? sampleRate = null,
        int? channels = null,
        int? bitDepth = null,
        int? bitrate = null,
        bool includeContainer = true,
        bool includeAudioCodec = true,
        bool includeBitDepth = true,
        bool includeBitrate = true)
    {
        return new TrackStreamDto(
            path ?? Path.Combine(_faker.System.DirectoryPath(), _faker.System.FileName("flac")),
            libraryId ?? Guid.NewGuid(),
            albumId ?? Guid.NewGuid(),
            artistId ?? Guid.NewGuid(),
            includeContainer ? (container ?? _faker.PickRandom("FLAC", "MP3", "OGG", "MP4", "AAC", "WAV")) : null,
            includeAudioCodec ? (audioCodec ?? _faker.PickRandom("FLAC", "MP3", "AAC", "ALAC", "PCM")) : null,
            durationInSeconds ?? _faker.Random.Int(1, 3600),
            sampleRate ?? _faker.PickRandom(44100, 48000, 96000),
            channels ?? _faker.PickRandom(1, 2, 6),
            includeBitDepth ? (bitDepth ?? _faker.Random.Int(8, 32)) : null,
            includeBitrate ? (bitrate ?? _faker.Random.Int(96, 1411)) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="TrackStreamDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TrackStreamDto"/> instances.</returns>
    public List<TrackStreamDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
