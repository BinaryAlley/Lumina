#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Contracts.Requests.UsersManagement.Settings;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Contracts.Fixtures.Core.Requests.UsersManagement.Settings;

/// <summary>
/// Fixture class for the <see cref="UpdatePlaybackStateRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="UpdatePlaybackStateRequest"/>.
    /// </summary>
    /// <param name="volume">Optional. The last volume the player reported, as a value between 0 and 1.</param>
    /// <param name="isMuted">Optional. The last mute state the player reported.</param>
    /// <param name="includeVolume">Whether the volume should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsMuted">Whether the mute state should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="UpdatePlaybackStateRequest"/>.</returns>
    public UpdatePlaybackStateRequest Create(
        double? volume = null,
        bool? isMuted = null,
        bool includeVolume = true,
        bool includeIsMuted = true)
    {
        return new UpdatePlaybackStateRequest(
            includeVolume ? volume ?? _faker.Random.Double(0, 1) : null,
            includeIsMuted ? isMuted ?? _faker.Random.Bool() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdatePlaybackStateRequest"/> instances.
    /// </summary>
    /// <param name="count">Number of elements to create.</param>
    /// <returns>The created list.</returns>
    public List<UpdatePlaybackStateRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
