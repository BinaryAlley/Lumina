#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Responses.UsersManagement.Settings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Responses.UsersManagement.Settings;

/// <summary>
/// Fixture class for the <see cref="PlaybackStateResponse"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaybackStateResponseFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaybackStateResponse"/>.
    /// </summary>
    /// <param name="volume">Optional. The last volume the player reported, as a value between 0 and 1.</param>
    /// <param name="isMuted">Optional. The last mute state the player reported.</param>
    /// <returns>The created <see cref="PlaybackStateResponse"/>.</returns>
    public PlaybackStateResponse Create(
        double? volume = null,
        bool? isMuted = null)
    {
        return new PlaybackStateResponse(
            volume ?? _faker.Random.Double(0, 1),
            isMuted ?? _faker.Random.Bool());
    }

    /// <summary>
    /// Creates a list of <see cref="PlaybackStateResponse"/> instances.
    /// </summary>
    /// <param name="count">Number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<PlaybackStateResponse> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
