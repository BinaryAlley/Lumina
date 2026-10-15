#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.MediaLibrary.AudioLibrary.MusicLibrary.Playback;

/// <summary>
/// Fixture class for the <see cref="PlaybackCapabilityDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackCapabilityDtoFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaybackCapabilityDto"/>.
    /// </summary>
    /// <param name="container">Optional. The container the player can demux.</param>
    /// <param name="codec">Optional. The codec the player can decode.</param>
    /// <returns>The created <see cref="PlaybackCapabilityDto"/>.</returns>
    public PlaybackCapabilityDto Create(
        string? container = null,
        string? codec = null)
    {
        return new PlaybackCapabilityDto(
            container ?? _faker.Random.AlphaNumeric(4).ToUpperInvariant(),
            codec ?? _faker.Random.AlphaNumeric(4).ToUpperInvariant());
    }

    /// <summary>
    /// Creates a list of <see cref="PlaybackCapabilityDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaybackCapabilityDto"/> instances.</returns>
    public List<PlaybackCapabilityDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
