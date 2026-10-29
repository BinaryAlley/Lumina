#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;
using Lumina.Domain.SharedKernel.Common.Enums.AudioLibrary;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Tracks;

/// <summary>
/// Fixture class for the <see cref="TrackStreamDataDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class TrackStreamDataDtoFixture
{
    private static readonly AudioFileFormat[] s_audioFileFormats = [AudioFileFormat.MP3, AudioFileFormat.FLAC, AudioFileFormat.OGG, AudioFileFormat.WAV];

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="TrackStreamDataDto"/>.
    /// </summary>
    /// <param name="path">Optional. The file system path of the audio file of the track.</param>
    /// <param name="format">Optional. The file format of the audio file of the track.</param>
    /// <param name="length">Optional. The length of the audio file of the track, in bytes.</param>
    /// <param name="lastModified">Optional. The date and time when the audio file of the track was last modified.</param>
    /// <returns>The created <see cref="TrackStreamDataDto"/>.</returns>
    public TrackStreamDataDto Create(
        string? path = null,
        AudioFileFormat? format = null,
        long? length = null,
        DateTimeOffset? lastModified = null)
    {
        return new TrackStreamDataDto(
            path ?? Path.Combine(_faker.System.DirectoryPath(), _faker.System.FileName("mp3")),
            format ?? _faker.PickRandom(s_audioFileFormats),
            length ?? _faker.Random.Long(1, 10_000_000),
            lastModified ?? _faker.Date.PastOffset());
    }

    /// <summary>
    /// Creates a list of <see cref="TrackStreamDataDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="TrackStreamDataDto"/> instances.</returns>
    public List<TrackStreamDataDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
