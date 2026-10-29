#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Core.UsersManagement.Settings.Commands.UpdatePlaybackState;

/// <summary>
/// Fixture class for the <see cref="UpdatePlaybackStateCommand"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class UpdatePlaybackStateCommandFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid command to update the playback state the player reports for the current user.
    /// </summary>
    /// <param name="volume">Optional. The last volume the player reported, as a value between 0 and 1.</param>
    /// <param name="isMuted">Optional. The last mute state the player reported.</param>
    /// <param name="includeVolume">Whether the volume should be included, or forced to <see langword="null"/>.</param>
    /// <param name="includeIsMuted">Whether the mute state should be included, or forced to <see langword="null"/>.</param>
    /// <returns>The created <see cref="UpdatePlaybackStateCommand"/>.</returns>
    public UpdatePlaybackStateCommand Create(
        double? volume = null,
        bool? isMuted = null,
        bool includeVolume = true,
        bool includeIsMuted = true)
    {
        return new UpdatePlaybackStateCommand(
            includeVolume ? volume ?? _faker.Random.Double(0, 1) : null,
            includeIsMuted ? isMuted ?? _faker.Random.Bool() : null);
    }

    /// <summary>
    /// Creates a list of <see cref="UpdatePlaybackStateCommand"/> instances.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="UpdatePlaybackStateCommand"/> instances.</returns>
    public List<UpdatePlaybackStateCommand> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
