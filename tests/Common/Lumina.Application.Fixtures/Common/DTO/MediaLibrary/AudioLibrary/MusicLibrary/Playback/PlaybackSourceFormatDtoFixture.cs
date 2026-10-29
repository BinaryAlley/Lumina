#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Fixture class for the <see cref="PlaybackSourceFormatDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackSourceFormatDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaybackSourceFormatDto"/>.
    /// </summary>
    /// <param name="container">Optional. The container of the audio file of the track.</param>
    /// <param name="codec">Optional. The codec of the audio of the track.</param>
    /// <param name="includeContainer">Whether the container should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeCodec">Whether the codec should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="PlaybackSourceFormatDto"/>.</returns>
    public PlaybackSourceFormatDto Create(
        string? container = null,
        string? codec = null,
        bool includeContainer = true,
        bool includeCodec = true)
    {
        return new PlaybackSourceFormatDto(
            includeContainer ? (container ?? _faker.Random.AlphaNumeric(4).ToUpperInvariant()) : null,
            includeCodec ? (codec ?? _faker.Random.AlphaNumeric(4).ToUpperInvariant()) : null);
    }

    /// <summary>
    /// Creates a list of <see cref="PlaybackSourceFormatDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaybackSourceFormatDto"/> instances.</returns>
    public List<PlaybackSourceFormatDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
