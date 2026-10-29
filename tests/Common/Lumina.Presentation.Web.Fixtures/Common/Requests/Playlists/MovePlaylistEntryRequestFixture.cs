#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Presentation.Web.Common.Requests.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Presentation.Web.Fixtures.Common.Requests.Playlists;

/// <summary>
/// Fixture class for the <see cref="MovePlaylistEntryRequest"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class MovePlaylistEntryRequestFixture
{
    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a new <see cref="MovePlaylistEntryRequest"/> instance with randomized test data.
    /// </summary>
    /// <param name="targetPosition">Optional new zero based position of the entry.</param>
    /// <param name="includeTargetPosition">Whether the target position should be included, or forced to <see langword="null"/>.</param>
    /// <returns>A configured <see cref="MovePlaylistEntryRequest"/> instance.</returns>
    public MovePlaylistEntryRequest Create(
        int? targetPosition = null,
        bool includeTargetPosition = true)
    {
        return new MovePlaylistEntryRequest(
            TargetPosition: includeTargetPosition ? (targetPosition ?? _faker.Random.Int(0, 1_000)) : null);
    }

    /// <summary>
    /// Creates multiple <see cref="MovePlaylistEntryRequest"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="MovePlaylistEntryRequest"/> instances.</returns>
    public List<MovePlaylistEntryRequest> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
