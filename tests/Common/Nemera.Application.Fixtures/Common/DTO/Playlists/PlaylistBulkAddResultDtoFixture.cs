#region ========================================================================= USING =====================================================================================
using Bogus;
using Lumina.Application.Common.DTO.Playlists;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
#endregion

namespace Lumina.Application.Fixtures.Common.DTO.Playlists;

/// <summary>
/// Fixture class for the <see cref="PlaylistBulkAddResultDto"/> record.
/// </summary>
[ExcludeFromCodeCoverage]
public class PlaylistBulkAddResultDtoFixture
{
    private const long MAXIMUM_RANDOM_POSITION = 1_000_000_000L;

    private readonly Faker _faker = new();

    /// <summary>
    /// Creates a random valid <see cref="PlaylistBulkAddResultDto"/>.
    /// </summary>
    /// <param name="firstPosition">Optional. The sparse order key of the first appended entry. Defaults to a bounded random key.</param>
    /// <param name="addedCount">Optional. The number of entries appended. Defaults to a bounded random count.</param>
    /// <returns>The created <see cref="PlaylistBulkAddResultDto"/>.</returns>
    public PlaylistBulkAddResultDto Create(
        long? firstPosition = null,
        int? addedCount = null)
    {
        return new PlaylistBulkAddResultDto(
            firstPosition ?? _faker.Random.Long(0, MAXIMUM_RANDOM_POSITION),
            addedCount ?? _faker.Random.Int(0, 1_000));
    }

    /// <summary>
    /// Creates a list of <see cref="PlaylistBulkAddResultDto"/> instances with randomized test data.
    /// </summary>
    /// <param name="count">Number of instances to create.</param>
    /// <returns>List of configured <see cref="PlaylistBulkAddResultDto"/> instances.</returns>
    public List<PlaylistBulkAddResultDto> CreateMany(int count = 3)
    {
        return [.. Enumerable.Range(0, count).Select(_ => Create())];
    }
}
